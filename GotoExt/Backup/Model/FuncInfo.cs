using System.Text.RegularExpressions;

namespace GotoExt.Model
{
    /// <summary>
    /// Func相关信息
    /// </summary>
    public class FuncInfo
    {
        /// <summary>
        /// 子系统编码：Gtxt
        /// </summary>
        public string AppCode => Regex.Match(Namespace, @"(?<=mysoft.)[a-z]+(?=\.)", RegexOptions.IgnoreCase).Value;

        /// <summary>
        /// 项目名称：Mysoft.Gtxt.GtFaMng
        /// </summary>
        public string ProjName => Regex.Match(Namespace, @"([a-z]+\.){2}[a-z]+", RegexOptions.IgnoreCase).Value;

        /// <summary>
        /// 命名空间：Mysoft.Gtxt.GtFaMng.AppServices
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 类名：GtFaAppService
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// 方法名：Save
        /// </summary>
        public string Func { get; set; }

        /// <summary>
        /// 参数个数
        /// </summary>
        public int ParamNum { get; set; }

        /// <summary>
        /// 返回值类型
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// 方法完全限定名：Mysoft.Gtxt.GtFaMng.AppServices.GtFaAppService.Save
        /// </summary>
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace) ||
                    string.IsNullOrEmpty(Class) ||
                    string.IsNullOrEmpty(Func))
                {
                    return "";
                }

                return $@"{Namespace}.{Class}.{Func}";
            }
        }
    }
}
