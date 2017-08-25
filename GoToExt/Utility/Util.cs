﻿using System.IO;
using System.Xml;

namespace GoToExt
{
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
    }
}