using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using GotoExt.Common;
using GotoExt.Model;

namespace GotoExt.Biz
{
    /// <summary>
    /// ServiceAPI相关操作
    /// </summary>
    public class ToSQLBiz : BaseBiz
    {
        private FuncInfo _funcInfo;

        public ToSQLBiz(DTE2 dte) : base(dte)
        {
            _funcInfo = new FuncInfo();
        }

        /// <summary>
        /// 转到定义
        /// </summary>
        public override void Run()
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

            Window win = Dte.ItemOperations.OpenFile(path);
            if (!ToSQL(win, sql))
            {
                MessageBox.Show("未找到匹配的SQL！");
                return;
            }
        }

        #region 私有方法

        /// <summary>
        /// 获取触发点字符串
        /// </summary>
        /// <returns></returns>
        private string GetSelection()
        {
            // 触发内容
            string code = ExtUtil.GetSelection(Dte);

            // 没有选中内容则获取当前行代码
            if (string.IsNullOrEmpty(code))
            {
                code = ExtUtil.GetSelectString(Dte);
            }
            
            return code;
        }

        /// <summary>
        /// 解析代码设置目标对象
        /// </summary>
        private void AnalyzeInfo()
        {
            ExtUtil.AnalyzeInfo(Dte, _funcInfo);
        }
        
        /// <summary>
        /// 获取资源文件完整路径
        /// </summary>
        /// <returns></returns>
        private string GetFullPath(string sql)
        {
            string slnPath = Dte.Solution?.FullName;
            if (string.IsNullOrEmpty(slnPath))
            {
                return "";
            }

            // 解决方案根路径
            string rootPath = slnPath.Substring(0, slnPath.LastIndexOf('\\'));

            // 找到“资源文件”文件夹
            List<string> resDirs = GetResDir(rootPath);

            // config配置文件
            List<string> files = GetConfig(resDirs);

            // 匹配sql关键字
            foreach (string file in files)
            {
                var doc = Util.ReadXml(file);

                XmlNode node = doc.SelectSingleNode($"//XmlCommand[@Name='{sql}']");
                if (node != null)
                {
                    return file;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取所有资源文件夹
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        private List<string> GetResDir(string rootPath)
        {
            List<string> dirs = Util.GetDirPathList(rootPath, "*", false);

            // 找到“资源文件”文件夹
            foreach (string dir in dirs)
            {
                if (dir.Contains("资源文件"))
                {
                    return Util.GetDirPathList(dir, "Mysoft.*.Resource", false);
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// 获取所有资源配置文件
        /// </summary>
        /// <param name="resDirs"></param>
        /// <returns></returns>
        private List<string> GetConfig(List<string> resDirs)
        {
            List<string> files = new List<string>();

            foreach (string resDir in resDirs)
            {
                string path = $@"{resDir}\XmlCommand";
                files.AddRange(Util.GetFilePathList(path, "*.config", true));
            }

            return files;
        }

        /// <summary>
        /// 定位SQL
        /// </summary>
        /// <param name="win"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool ToSQL(Window win, string sql)
        {
            string pattern = $@"\b{sql}\b";
            return ExtUtil.ToCode(win, pattern);
        }

        #endregion
    }
}
