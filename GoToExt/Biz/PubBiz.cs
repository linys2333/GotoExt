using EnvDTE;
using EnvDTE80;
using System.Text.RegularExpressions;

namespace GoToExt
{
    /// <summary>
    /// ServiceAPI相关操作
    /// </summary>
    public class PubBiz
    {
        private DTE2 _dte;

        public PubBiz(DTE2 dte)
        {
            _dte = dte;
        }

        /// <summary>
        /// 获取触发行的代码
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {
            // 触发内容
            var selection = (TextSelection)_dte.ActiveDocument.Selection;
            string code = selection.Text;

            // 没有选中内容则获取当前行代码
            if (string.IsNullOrEmpty(code))
            {
                // 触发起始点
                EditPoint editPoint = selection.AnchorPoint.CreateEditPoint();

                // 选中触发行
                code = editPoint.GetLines(editPoint.Line, editPoint.Line + 1);
            }

            // 多行转成一行并去掉多余空格
            code = Regex.Replace(code, @"\s+", s => "");

            return code;
        }

        /// <summary>
        /// 解析触发点的代码模型
        /// </summary>
        /// <param name="serviceFunc"></param>
        public void AnalyzeInfo(FuncInfo serviceFunc)
        {
            var codeModel = (FileCodeModel2)_dte.ActiveDocument.ProjectItem.FileCodeModel;
            var selection = (TextSelection)_dte.ActiveDocument.Selection;

            // 触发的方法信息
            var codeFunc = (CodeFunction2)codeModel.CodeElementFromPoint(selection.AnchorPoint, vsCMElement.vsCMElementFunction);
            var codeClass = (CodeClass2)codeFunc.Parent;

            serviceFunc.Namespace = codeClass.Namespace.Name;
            serviceFunc.Class = codeClass.Name;
            serviceFunc.Func = codeFunc.Name;
            serviceFunc.ParamNum = codeFunc.Parameters.Count;
            serviceFunc.IsPublic = codeFunc.Access == vsCMAccess.vsCMAccessPublic;
        }

        /// <summary>
        /// 匹配代码
        /// </summary>
        /// <returns></returns>
        public bool ToCode(Window win, string pattern)
        {
            var doc = (TextDocument)win.Document.Object("TextDocument");

            EditPoint editPoint = doc.StartPoint.CreateEditPoint();
            EditPoint endPoint = null;
            TextRanges tags = null;

            bool find = editPoint.FindPattern(pattern, (int)(vsFindOptions.vsFindOptionsFromStart |
                vsFindOptions.vsFindOptionsRegularExpression), ref endPoint, ref tags);

            if (find && tags != null)
            {
                doc.Selection.MoveToPoint(endPoint, false);
                doc.Selection.MoveToPoint(editPoint, true);
            }

            return find;
        }

        /// <summary>
        /// 匹配代码
        /// </summary>
        /// <returns></returns>
        public string FindCode(Window win, string pattern)
        {
            var doc = (TextDocument)win.Document.Object("TextDocument");

            EditPoint editPoint = doc.StartPoint.CreateEditPoint();
            EditPoint endPoint = null;
            TextRanges tags = null;

            bool find = editPoint.FindPattern(pattern, (int)(vsFindOptions.vsFindOptionsFromStart |
                vsFindOptions.vsFindOptionsRegularExpression), ref endPoint, ref tags);

            if (find && tags != null)
            {
                return editPoint.GetText(endPoint);
            }

            return "";
        }
    }
}
