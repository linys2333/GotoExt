using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using GotoExt.Common;
using GotoExt.Model;
using System.Linq;

namespace GotoExt.Biz
{
    /// <summary>
    /// ToAPI相关业务
    /// </summary>
    public class ToAPIBiz : BaseBiz
    {
        private ServiceFunc _funcInfo;

        public ToAPIBiz(DTE2 dte) : base(dte)
        {
            _funcInfo = new ServiceFunc();
        }

        /// <summary>
        /// 转到定义
        /// </summary>
        public override void Goto()
        {
            // 解析
            string selection = DteUtil.GetSelectRowCode(Dte);
            if (string.IsNullOrWhiteSpace(selection))
            {
                return;
            }
            AnalyzeInfo(selection);

            // 校验
            if (string.IsNullOrEmpty(_funcInfo.FullName))
            {
                MessageBox.Show("该方法缺少引用，请检查require！");
                return;
            }

            // 查找
            string path = GetFullPath();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("未找到对应的后端文件！");
                return;
            }

            Window win = Dte.ItemOperations.OpenFile(path);
            if (!DteUtil.ToCode(win, _funcInfo))
            {
                if (_funcInfo.ParamNum > -1)
                {
                    // 如果带参数匹配不到，那么仅匹配方法名
                    _funcInfo.ParamNum = -1;

                    if (ToFunc(win))
                    {
                        MessageBox.Show("未找到匹配的方法，已定位到同名方法！");
                        return;
                    }
                }

                MessageBox.Show("未找到匹配的方法！");
                return;
            }
        }

        #region 私有方法

        /// <summary>
        /// 解析代码设置目标对象
        /// </summary>
        /// <param name="code">例如：[return ]appService.GetGtPackDtl(packGUID, gtLevel)</param>
        private void AnalyzeInfo(string code)
        {
            // 删掉回调函数
            code = Regex.Replace(code, @"\.then\b.*$", s => "");

            string service = Regex.Match(code, @"\b\w+Service(?=\.)").Value;
            if (string.IsNullOrEmpty(service))
            {
                return;
            }

            string js = GetJs(service);
            if (string.IsNullOrEmpty(js))
            {
                return;
            }

            _funcInfo.Namespace = js.Substring(0, js.LastIndexOf('.'));
            _funcInfo.Class = js.Substring(js.LastIndexOf('.') + 1);
            _funcInfo.Func = Regex.Match(code, @"(?<=Service\.)\w+\b").Value;

            // 采用正则平衡组匹配嵌套参数
            string param = Regex.Match(code,
                $@"(?<={_funcInfo.Func}\s*?\()(?>[^\(\)]+|\((?<sign>)|\)(?<-sign>))*(?(sign)(?!))(?=\))").Value;

            if (string.IsNullOrEmpty(param))
            {
                _funcInfo.ParamNum = 0;
            }
            else
            {
                param = FilterParam(param);
                _funcInfo.ParamNum = param.Split(',').Length;
            }
        }

        /// <summary>
        /// 根据service名称获取require引用代码
        /// </summary>
        /// <param name="service"></param>
        /// <returns>例如：Mysoft.Gtxt.GtFaMng.AppServices.GtFaAppService</returns>
        private string GetJs(string service)
        {
            // appService = require("Mysoft.Gtxt.GtFaMng.AppServices.GtFaAppService")
            string text = DteUtil.FindCode(Dte.ActiveWindow, $@"\b{service}\s+=\s+require\([""|'].+?[""|']\)");

            string pattern = $@"(?<=\brequire\([""|']).+?(?=[""|']\))";
            string value = Regex.Match(text, pattern, RegexOptions.IgnoreCase).Value;

            return value.Trim();
        }

        /// <summary>
        /// 获取后端代码完整路径
        /// </summary>
        /// <returns></returns>
        private string GetFullPath()
        {
            string slnPath = Dte.Solution?.FullName;
            if (string.IsNullOrEmpty(slnPath))
            {
                return "";
            }

            // xxx\00_根目录\Web_Gtxt.csproj
            string projPath = Dte.ActiveDocument.ProjectItem.ContainingProject.FullName;

            // 反序列化会报错，故直接操作XmlDocument
            XmlDocument doc = Util.ReadXml(projPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", doc.DocumentElement.Attributes["xmlns"].Value);

            XmlNode node = doc.SelectSingleNode($"//ns:ProjectReference[ns:Name='{_funcInfo.ProjName}']", nsmgr);

            // ..\01 基础数据设置\Mysoft.Gtxt.BaseSet\Mysoft.Gtxt.BaseSet.csproj
            string refPath = node?.Attributes?["Include"].Value ?? "";

            if (!string.IsNullOrEmpty(refPath))
            {
                string rootPath = slnPath.Substring(0, slnPath.LastIndexOf('\\'));
                refPath = Regex.Match(refPath, @"(?<=\.\.\\).+(?=\\[^\\]+\.csproj)").Value;

                string path = $@"{rootPath}\{refPath}\{_funcInfo.FilePath2}.cs";

                if (File.Exists(path))
                {
                    return path;
                }
            }

            //如果没有找到，直接尝试全文搜索
            var file = Util.GetFilePathList(Path.GetDirectoryName(slnPath), _funcInfo.Class + ".cs", true);

            return file.FirstOrDefault() ?? "";
        }

        /// <summary>
        /// 递归过滤掉嵌套参数
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string FilterParam(string param)
        {
            string filter = Regex.Replace(param, @"\([^\(\)]*\)", m => "");
            if (param != filter)
            {
                return FilterParam(filter);
            }
            return filter;
        }

        /// <summary>
        /// 定位方法
        /// </summary>
        /// <returns></returns>
        private bool ToFunc(Window win)
        {
            // 匹配方法定义代码
            string pattern = @"\w_ \.\<\>\[\]\t\n";
            switch (_funcInfo.ParamNum)
            {
                // -1不匹配参数个数
                case -1:
                    pattern = $@"public\svirtual\s[a-zA-Z]+\s{_funcInfo.Func}\([^\)]*\)";
                    break;
                case 0:
                    pattern = $@"public\svirtual\s[a-zA-Z]+\s{_funcInfo.Func}\(\)";
                    break;
                case 1:
                    pattern = $@"public\svirtual\s[a-zA-Z]+\s{_funcInfo.Func}\([{pattern}]+\)";
                    break;
                default:
                    pattern =
                        $@"public\svirtual\s[a-zA-Z]+\s{_funcInfo.Func}\(([{pattern}]+,){{{_funcInfo.ParamNum - 1}}}[{pattern}]+\)";
                    break;
            }

            return DteUtil.ToCode(win, pattern);
        }

        #endregion
    }
}
