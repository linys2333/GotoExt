using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace GotoExt.Common
{
    /// <summary>
    /// 通用类
    /// </summary>
    public static class Util
    {        
        /// <summary>
        /// 读取xml文本
        /// <para>IOFilePathException异常</para>
        /// <para>IOException</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static XmlDocument ReadXml(string path)
        {
            if (!File.Exists(path))
            {
                throw new IOException($"文件路径【{path}】不存在！");
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            return xmlDoc;
        }

        /// <summary>
        /// 获取文件后缀
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetExtName(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            if (index == -1 || index == fileName.Length - 1)
            {
                return "";
            }
            return fileName.Substring(index + 1);
        }

        /// <summary>
        /// 获取文件路径列表
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="isAllDir">是否搜索所有文件夹</param>
        /// <returns></returns>
        public static List<string> GetFilePathList(string path, string searchPattern, bool isAllDir)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*.*";
            }

            return Directory.Exists(path)
                ? Directory.EnumerateFiles(path, searchPattern, isAllDir
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly).ToList()
                : new List<string>();
        }

        /// <summary>
        /// 获取文件夹路径列表
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="isAllDir">是否搜索所有文件夹</param>
        /// <returns></returns>
        public static List<string> GetDirPathList(string path, string searchPattern, bool isAllDir)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            return Directory.Exists(path)
                ? Directory.EnumerateDirectories(path, searchPattern, isAllDir
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly).ToList()
                : new List<string>();
        }
    }
}