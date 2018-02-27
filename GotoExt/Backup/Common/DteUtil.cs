using System;
using EnvDTE;
using EnvDTE80;
using System.Text.RegularExpressions;
using GotoExt.Model;

namespace GotoExt.Common
{
    /// <summary>
    /// 插件公共类
    /// </summary>
    public static class DteUtil
    {
        /// <summary>
        /// 获取选中的代码
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static string GetSelection(DTE2 dte)
        {
            // 触发内容
            var selection = (TextSelection)dte.ActiveDocument.Selection;
            string code = selection.Text;
            
            // 多行转成一行
            code = Regex.Replace(code, @"\s+", s => " ");

            return code.Trim();
        }

        /// <summary>
        /// 获取触发行的代码
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static string GetSelectRowCode(DTE2 dte)
        {
            // 触发内容
            var selection = (TextSelection)dte.ActiveDocument.Selection;

            // 触发起始点
            EditPoint editPoint = selection.AnchorPoint.CreateEditPoint();

            // 选中触发行
            string code = editPoint.GetLines(editPoint.Line, editPoint.Line + 1);

            // 多行转成一行
            code = Regex.Replace(code, @"\s+", s => " ");

            return code.Trim();
        }

        /// <summary>
        /// 获取触发点的字符串
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static string GetSelectString(DTE2 dte)
        {
            // 触发内容
            var selection = (TextSelection)dte.ActiveDocument.Selection;

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

            // 触发点字符串
            string code = startPoint.GetText(endPoint);
            if (!code.StartsWith(@"""") || !code.EndsWith(@""""))
            {
                code = "";
            }

            // 多行转成一行并去掉多余符号
            return Regex.Replace(code, @"[""|\s]+", s => " ").Trim();
        }

        /// <summary>
        /// 解析触发点的方法模型
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="serviceFunc"></param>
        public static void AnalyzeFunc(DTE2 dte, ref FuncInfo serviceFunc)
        {
            var codeModel = (FileCodeModel2)dte.ActiveDocument.ProjectItem.FileCodeModel;
            var selection = (TextSelection)dte.ActiveDocument.Selection;

            // 触发的方法信息
            var codeFunc = (CodeFunction2) codeModel.CodeElementFromPoint(selection.AnchorPoint, vsCMElement.vsCMElementFunction);
            var codeClass = (CodeClass2)codeFunc.Parent;

            serviceFunc.Namespace = codeClass.Namespace.Name;
            serviceFunc.Class = codeClass.Name;
            serviceFunc.Func = codeFunc.Name;
            serviceFunc.ParamNum = codeFunc.Parameters.Count;
            serviceFunc.IsPublic = codeFunc.Access == vsCMAccess.vsCMAccessPublic;
        }

        /// <summary>
        /// 单纯匹配代码
        /// </summary>
        /// <param name="win"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool ToCode(Window win, string pattern)
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
        /// 匹配代码模型
        /// </summary>
        /// <param name="win"></param>
        /// <param name="funcInfo"></param>
        /// <returns></returns>
        public static bool ToCode(Window win, FuncInfo funcInfo)
        {
            // 查找命名空间
            Func<CodeElements, CodeNamespace> findNamespace = source =>
            {
                if (source == null)
                {
                    return null;
                }

                for (int i = 1; i <= source.Count; i++)
                {
                    CodeElement elem = source.Item(i);

                    if (elem.Kind != vsCMElement.vsCMElementNamespace)
                    {
                        continue;
                    }

                    var target = (CodeNamespace) elem;
                    if (!target.Name.Equals(funcInfo.Namespace, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    return target;
                }

                return null;
            };

            // 查找类
            Func<CodeElements, CodeClass> findClass = source =>
            {
                if (source == null)
                {
                    return null;
                }

                for (int i = 1; i <= source.Count; i++)
                {
                    CodeElement elem = source.Item(i);

                    if (elem.Kind != vsCMElement.vsCMElementClass)
                    {
                        continue;
                    }

                    var target = (CodeClass)elem;
                    if (!target.Name.Equals(funcInfo.Class, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    return target;
                }

                return null;
            };

            // 查找方法
            Func<CodeElements, CodeFunction> findFunc = source =>
            {
                if (source == null)
                {
                    return null;
                }

                for (int i = 1; i <= source.Count; i++)
                {
                    CodeElement elem = source.Item(i);

                    if (elem.Kind != vsCMElement.vsCMElementFunction)
                    {
                        continue;
                    }

                    var target = (CodeFunction)elem;
                    if (target.Name.Equals(funcInfo.Func, StringComparison.OrdinalIgnoreCase) &&
                        (funcInfo.ParamNum == -1 || target.Parameters.Count == funcInfo.ParamNum))
                    {
                        return target;
                    }
                }

                return null;
            };

            // 获取目标方法
            FileCodeModel codeModel = win.Document.ProjectItem.FileCodeModel;
            CodeNamespace namesp = findNamespace(codeModel?.CodeElements);
            CodeClass cls = findClass(namesp?.Children);
            CodeFunction func = findFunc(cls?.Children);
            
            if (func == null)
            {
                return false;
            }

            // 选中
            EditPoint startPoint = func.StartPoint.CreateEditPoint();
            EditPoint endPoint = null;
            
            bool find = startPoint.FindPattern($@"\b{funcInfo.Func}\b", (int)(vsFindOptions.vsFindOptionsNone | 
                vsFindOptions.vsFindOptionsRegularExpression), ref endPoint);
            
            if (find)
            {
                var doc = (TextDocument)win.Document.Object("TextDocument");
                doc.Selection.MoveToPoint(endPoint, false);
                doc.Selection.MoveToPoint(startPoint, true);
            }

            return find;
        }

        /// <summary>
        /// 查找代码
        /// </summary>
        /// <param name="win"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string FindCode(Window win, string pattern)
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
