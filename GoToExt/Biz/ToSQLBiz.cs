using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using EnvDTE80;

namespace GoToExt
{
    /// <summary>
    /// ServiceAPI相关操作
    /// </summary>
    public class ToSQLBiz
    {
        private FuncInfo _funcInfo;
        private readonly DTE2 _dte;
        private readonly PubBiz _pubBiz;

        public ToSQLBiz(DTE2 dte)
        {
            _dte = dte;
            _pubBiz = new PubBiz(_dte);
            _funcInfo = new FuncInfo();
        }

        /// <summary>
        /// 转到定义
        /// </summary>
        public void Go()
        {
            // 解析
            string sql = GetSelection();
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }
            AnalyzeInfo();
            
            // 查找
            string path = GetFullPath(sql);
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("未找到对应的资源文件！");
                return;
            }

            Window win = _dte.ItemOperations.OpenFile(path);
            if (!ToSQL(win, sql))
            {
                MessageBox.Show("未找到匹配的SQL！");
                return;
            }
        }

        #region 私有方法

        /// <summary>
        /// 获取触发行的代码
        /// </summary>
        /// <returns></returns>
        private string GetSelection()
        {
            // 触发内容
            var selection = (TextSelection)_dte.ActiveDocument.Selection;
            string code = selection.Text;

            // 没有选中内容则获取当前行代码
            if (string.IsNullOrEmpty(code))
            {
                // 触发起始点
                var point = selection.AnchorPoint;
                EditPoint editPoint = point.CreateEditPoint();
                EditPoint startPoint = point.CreateEditPoint();
                EditPoint endPoint = point.CreateEditPoint();

                // 寻找字符串起始点
                for (int i = 0; i < 100; i++)
                {
                    startPoint.MoveToLineAndOffset(editPoint.Line, startPoint.LineCharOffset - 1);

                    string str = editPoint.GetText(startPoint);

                    if (startPoint.LineCharOffset <= 1 || str.StartsWith(@""""))
                    {
                        break;
                    }
                }

                // 寻找字符串终止点
                for (int i = 0; i < 100; i++)
                {
                    endPoint.MoveToLineAndOffset(editPoint.Line, endPoint.LineCharOffset + 1);

                    string str = editPoint.GetText(endPoint);

                    if (endPoint.LineCharOffset > editPoint.LineLength || str.EndsWith(@""""))
                    {
                        break;
                    }
                }

                code = startPoint.GetText(endPoint);
                if (!code.StartsWith(@"""") || !code.EndsWith(@""""))
                {
                    code = "";
                }
            }

            // 多行转成一行并去掉多余符号
            return Regex.Replace(code, @"[""|\s]+", s => " ").Trim();
        }

        /// <summary>
        /// 解析代码设置目标对象
        /// </summary>
        private void AnalyzeInfo()
        {
            _pubBiz.AnalyzeInfo(_funcInfo);
        }
        
        /// <summary>
        /// 获取资源文件完整路径
        /// </summary>
        /// <returns></returns>
        private string GetFullPath(string sql)
        {
            string slnPath = _dte.Solution?.FullName;
            if (string.IsNullOrEmpty(slnPath))
            {
                return "";
            }

            string rootPath = slnPath.Substring(0, slnPath.LastIndexOf('\\'));

            var dirs = Directory.EnumerateDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();

            List<string> files = new List<string>();
            foreach (string dir in dirs)
            {
                if (dir.Contains("资源文件"))
                {
                    string path = $@"{dir}\Mysoft.{_funcInfo.AppCode}.Resource\XmlCommand";
                    files.AddRange(GetConfig(path));

                    path = $@"{dir}\{_funcInfo.ProjName}.Resource\XmlCommand";
                    files.AddRange(GetConfig(path));

                    break;
                }
            }

            var docs = new Dictionary<string, XmlDocument>();
            foreach (string file in files)
            {
                docs.Add(file, Util.ReadXml(file));
            }

            foreach (var doc in docs)
            {
                XmlNode node = doc.Value.SelectSingleNode($"//XmlCommand[@Name='{sql}']");
                if (node != null)
                {
                    return doc.Key;
                }
            }

            return "";
        }

        /// <summary>
        /// 获取所有资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string> GetConfig(string path)
        {
            return Directory.Exists(path)
                ? Directory.EnumerateFiles(path, "*.config", SearchOption.AllDirectories).ToList()
                : new List<string>();
        }

        private bool ToSQL(Window win, string sql)
        {
            string pattern = $@"\b{sql}\b";
            return _pubBiz.ToCode(win, pattern);
        }

        #endregion
    }
}
