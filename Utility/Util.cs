using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ToAPI
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
    }
}