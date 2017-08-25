using EnvDTE;
using EnvDTE80;

namespace GoToExt
{
    /// <summary>
    /// ServiceAPI相关操作
    /// </summary>
    public class FindUsedBiz
    {
        private FuncInfo _funcInfo;
        private DTE2 _dte;

        public FindUsedBiz(DTE2 dte)
        {
            _dte = dte;
            _funcInfo = new FuncInfo();
        }

        /// <summary>
        /// 查找引用
        /// </summary>
        /// <param name="isMatchWhole">是否完全匹配</param>
        public void FindUsed(bool isMatchWhole)
        {
            // 解析
            AnalyzeInfo();

            // 校验
            if (string.IsNullOrEmpty(_funcInfo.FullName))
            {
                return;
            }

            if (!_funcInfo.Namespace.Contains("Services") ||
                !_funcInfo.Class.EndsWith("Service") ||
                !_funcInfo.IsPublic)
            {
                return;
            }

            // 查找
            ExecFind(isMatchWhole);
        }

        #region 私有方法

        /// <summary>
        /// 利用CodeModel设置目标对象
        /// </summary>
        /// <param name="code"></param>
        private void AnalyzeInfo()
        {
            var codeModel = (FileCodeModel2)_dte.ActiveDocument.ProjectItem.FileCodeModel;
            var selection = (TextSelection)_dte.ActiveDocument.Selection;

            try
            {
                // 触发的ServiceAPI
                var codeFunc = (CodeFunction2)codeModel.CodeElementFromPoint(selection.AnchorPoint, vsCMElement.vsCMElementFunction);
                var codeClass = (CodeClass2)codeFunc.Parent;

                _funcInfo.Namespace = codeClass.Namespace.Name;
                _funcInfo.Class = codeClass.Name;
                _funcInfo.Func = codeFunc.Name;
                _funcInfo.ParamNum = codeFunc.Parameters.Count;
                _funcInfo.IsPublic = codeFunc.Access == vsCMAccess.vsCMAccessPublic;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // 没找到对应元素
                throw;
            }
        }

        /// <summary>
        /// 执行查找
        /// </summary>
        /// <param name="isMatchWhole">是否完全匹配</param>
        private void ExecFind(bool isMatchWhole)
        {
            string pattern = isMatchWhole ?
                $"{_funcInfo.Class}.{_funcInfo.Func}" :
                $".{_funcInfo.Func}";

            var findWin = (Find2)_dte.Find;

            findWin.Action = vsFindAction.vsFindActionFindAll;
            findWin.Backwards = false;
            findWin.FilesOfType = "*.aspx;*.js";
            findWin.FindWhat = pattern;
            findWin.MatchCase = true;
            findWin.MatchInHiddenText = true;
            findWin.MatchWholeWord = true;
            findWin.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
            findWin.ResultsLocation = vsFindResultsLocation.vsFindResults2;
            findWin.SearchSubfolders = true;
            findWin.Target = vsFindTarget.vsFindTargetSolution;
            findWin.WaitForFindToComplete = false;

            findWin.Execute();

            // 还原部分设置，防止后续误操作导致卡顿
            findWin.Target = vsFindTarget.vsFindTargetCurrentDocument;
        }

        #endregion
    }
}
