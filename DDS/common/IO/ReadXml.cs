using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using OMS.common.Utilities;

namespace OMS.common.IO
{
    /// <summary>
    /// Summary description for Class1.
    /// How to use this file
    /// U would use new TReadXML(xmlfile,headerSection) to create a Ojbect
    /// or U could use SetInstantce(xmlFile) to create a Object,in the time the section is "config"-->this object could be get by getinstance
    /// or u could use setInstance(XmlFIle,headerSection)to create a object->this object could be get by getinstance
    /// if u don't setinstance,just use getinstance,in this time u could get (CurrenctFolder+config.xml,"config")
    /// </summary>
    public class XmlReader : IDisposable
    {
        #region Members

        private static XmlReader aReadXml = null;
        private static char[] SEPARATE ={ '.' };
        private XmlDocument xd;
        private string header;
        private string fileName;

        private XmlNode FHeaderNode;
        public XmlNode HeaderNode { get { return FHeaderNode; } }

        #endregion

        #region Dispose

        public void Dispose()
        {
            FHeaderNode = null;
            xd = null;
        }

        #endregion

        #region Constructor

        public XmlReader(string FileName, string headerstr)
        {
            try
            {
                xd = new XmlDocument();
                header = headerstr;
                fileName = FileName;
                xd.Load(fileName);
                FHeaderNode = xd.LastChild;
            }
            catch (Exception ex)
            {
                xd = null;
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion

        #region Singleton instance

        public static void SetInstance(string FileName, string HeadStr)
        {
            try
            {
                if (aReadXml != null)
                {
                    aReadXml.Dispose();
                    aReadXml = null;
                }
                aReadXml = new XmlReader(FileName, HeadStr);
            }
            catch (Exception ex)
            {
                aReadXml = null;
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public static void SetInstance(string FileName)
        {
            try
            {
                if (aReadXml != null)
                {
                    aReadXml.Dispose();
                    aReadXml = null;
                }
                aReadXml = new XmlReader(FileName, "config");
            }
            catch (Exception ex)
            {
                aReadXml = null;
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public static XmlReader DefaultInstance
        {
            get
            {
                try
                {
                    if (aReadXml != null)
                    {
                        return aReadXml;
                    }
                    else
                    {
                        string tempfile;
                        tempfile = System.IO.Directory.GetCurrentDirectory();
                        tempfile = tempfile + "\\config.xml";
                        aReadXml = new XmlReader(tempfile, "config");
                        return aReadXml;
                    }
                }
                catch (Exception ex)
                {
                    aReadXml = null;
                    TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    return null;
                }
            }
        }

        #endregion

        #region XML node

        public XmlNode GetNode(string apath)
        {
            try
            {
                string[] path = apath.Split(SEPARATE);
                XmlNode tempNode = HeaderNode;
                foreach (string onepath in path)
                {
                    if (onepath != header)
                    {
                        tempNode = GetSubNode(tempNode, onepath);
                    }
                    if (tempNode == null)
                        return null;
                }
                return tempNode;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return null;
            }
        }

        private XmlNode GetSubNode(XmlNode TopNode, string apath)
        {
            try
            {
                Int32 aInt = int.Parse(apath);
                if (aInt < 0 || aInt >= TopNode.ChildNodes.Count) return null;
                else return TopNode.ChildNodes[aInt];
            }
            catch
            {
                return TopNode.SelectSingleNode(apath);
            }
        }

        public bool GetNodeValue(string apath, ref string str)
        {
            str = "";
            XmlNode aNode;
            try
            {
                aNode = GetNode(apath);
                if (aNode == null) return false;
                else
                {
                    str = aNode.InnerText;
                    return true;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                str = "";
                return false;
            }
        }

        public bool GetNodeValue(XmlNode Node, ref object nodeValue)
        {
            bool result = false;
            if (Node != null)
            {
                try
                {
                    nodeValue = Node.InnerText;
                    //get real value with specific data type
                    XmlAttribute XaDt = Node.Attributes["dt"];
                    if ((XaDt != null) && (XaDt.Value.Length > 0))
                    {
                        switch (XaDt.Value[0])
                        {
                            case 'b': nodeValue = Convert.ToBoolean(nodeValue); break;
                            case 'i': nodeValue = Convert.ToInt32(nodeValue); break;
                            case 'f': nodeValue = Convert.ToDecimal(nodeValue); break;
                            default: nodeValue = Convert.ToString(nodeValue); break;
                        }
                    }
                }
                catch
                {
                    result = false; 
                }
            }

            return result;
        }

        public bool WriteNodeValue(string apath, string nodeValue)
        {
            XmlNode aNode;
            try
            {
                aNode = GetNode(apath);
                if (aNode == null) return false;
                else
                {
                    aNode.InnerText = nodeValue;
                    return true;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return false;
            }
        }

        public bool SaveToFile()
        {
            try
            {
                xd.Save(fileName);
                return true;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return false;
            }
        }

        public bool GetNodeAttribute(string apath, ref string str)
        {
            str = "";
            XmlNode aNode;
            string RealPath;
            string AttrName;
            int templen;
            try
            {
                templen = apath.LastIndexOf(SEPARATE[0]);
                RealPath = apath.Substring(0, templen);
                AttrName = apath.Substring(templen + 1, apath.Length - templen - 1);
                aNode = GetNode(RealPath);
                if (aNode != null)
                {
                    XmlAttribute aAttribute = aNode.Attributes[AttrName];
                    if (aAttribute != null)
                    {
                        str = aAttribute.Value;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                str = "";
            }

            return false;
        }

        public bool GetChildList(string aPath, ref List<XmlNode> ChildList)
        {
            try
            {
                XmlNode aNode = GetNode(aPath);
                if (aNode == null) return false;
                else
                {
                    if (aNode.HasChildNodes)
                    {
                        for (int i = 0; i < aNode.ChildNodes.Count; i++)
                        {
                            ChildList.Add(aNode.ChildNodes[i]);
                        }
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                ChildList.Clear();
                return false;
            }
        }

        public string ReadString(string section, string key)
        {
            try
            {
                XmlNode root = xd.SelectSingleNode(header);
                if (root != null)
                {
                    XmlNode item = root.SelectSingleNode(section);
                    if (item != null)
                    {
                        XmlNode node = item.SelectSingleNode(key);
                        if (node != null) return node.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return "";
        }

        #endregion
    }
}