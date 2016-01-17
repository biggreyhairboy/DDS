using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OMS.common.Utilities;

namespace OMS.common.IO
{
    public class IniReader : IDisposable
    {
        #region Members

        protected const string sectionGroupPattern = @"^[\s]*\[(?<SectionName>.*?)\][\s]*(?<SectionContent>[^\[]+)$";
        protected const string keyValuePattern = @"[\s]*(?<Key>.+?)[\s]*=[\s]*(?<Value>[\s]*[^\r]*)";

        protected static IniReader instance;

        protected string filePath = "";
        protected Dictionary<string, string> innerList;
        protected Dictionary<string, IniBlock> blockMap;

        #endregion

        #region Constructor & Destructor

        public IniReader(string path)
        {
            filePath = path;
            innerList = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            try
            {
                innerList.Clear();
                innerList = null;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion

        #region Singleton instance

        public static void SetInstance(string path)
        {
            try
            {
                if (instance != null)
                {
                    instance.Dispose();
                    instance = null;
                }
                instance = new IniReader(path);
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public static IniReader DefaultInstance
        {
            get
            {
                try
                {
                    if (instance == null)
                    {
                        instance = new IniReader("config.ini");
                    }
                    return instance;
                }
                catch (Exception ex)
                {
                    TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    return null;
                }
            }
        }

        public static IniReader GetInstance(string path)
        {
            try
            {
                if (instance == null)
                {
                    instance = new IniReader(path);
                }
                else
                {
                    if (instance.filePath != path)
                    {
                        instance.Dispose();
                        instance = null;
                        instance = new IniReader(path);
                    }
                }
                return instance;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return null;
            }
        }

        #endregion

        #region Read Write Value

        public void WriteValue(string section, string key, string value)
        {
            try
            {
                Kernel32.WritePrivateProfileString(section, key, value, filePath);
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public bool GetValue(string path, ref string value)
        {
            try
            {
                bool res = false;
                value = "";
                string pattern = @"\S\.\S";
                if (Regex.IsMatch(path, pattern))
                {
                    string[] key = path.Split(new char[] { '.' });

                    StringBuilder temp = new StringBuilder(255);

                    int i = Kernel32.GetPrivateProfileString(key[0], key[1], "", temp, 255, filePath);

                    value = temp.ToString();
                    temp = null;
                    key = null;
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                value = "";
                return false;
            }
        }

        #endregion

        #region Section relevant

        public Dictionary<string, IniBlock> GetIniBlock()
        {
            if (blockMap != null) return blockMap;
            if (IsPathExists(filePath))
            {
                blockMap = new Dictionary<string, IniBlock>(StringComparer.InvariantCultureIgnoreCase);
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string content = reader.ReadToEnd();
                    if (content != null && content.Trim() != "")
                    {
                        Regex regex = new Regex(sectionGroupPattern, RegexOptions.Multiline);
                        for (Match match = regex.Match(content.Trim()); match.Success; match = match.NextMatch())
                        {
                            string sectionName = match.Groups["SectionName"].Value;
                            string sectionContent = match.Groups["SectionContent"].Value;
                            ProcSectionGroup(sectionName, sectionContent);
                        }
                    }
                }

                return blockMap;
            }

            return null;
        }

        private void ProcSectionGroup(string section, string content)
        {
            if (section == null || section.Trim() == "") return;
            if (content == null || content.Trim() == "") return;

            IniBlock block = new IniBlock();
            block.Section = section;
            Regex regex = new Regex(keyValuePattern);
            for (Match match = regex.Match(content.Trim()); match.Success; match = match.NextMatch())
            {
                string key = match.Groups["Key"].Value;
                if (key.StartsWith(";")) continue;
                string value = match.Groups["Value"].Value;
                if (key != null && key.Trim() != "" && value != null) block.Add(key.Trim(), value.Trim());
            }

            if (block.IsValidBlock)
                blockMap[block.Section] = block;
        }

        public bool LoadSection(string section)
        {
            try
            {
                innerList.Clear();
                if (!IsPathExists(filePath))
                {
                    return false;
                }

                byte[] temBuffer = new byte[2048];
                char[] equal ={ '=' };
                int reslong = 0;
                int position;
                string resstr;
                string tempstr;
                string[] TempArray = new string[2];
                reslong = Kernel32.GetPrivateProfileSection(section, temBuffer, temBuffer.Length, filePath);
                try
                {
                    resstr = Encoding.Default.GetString(temBuffer, 0, reslong);
                }
                catch (Exception e)
                {
                    resstr = Encoding.ASCII.GetString(temBuffer, 0, reslong);
                    TLog.DefaultInstance.WriteLog(e.ToString(), LogType.ERROR);
                }
                temBuffer = null;
                position = resstr.IndexOf("\0");
                while (position >= 0)
                {
                    tempstr = resstr.Substring(0, position);
                    resstr = resstr.Substring(position + 1, resstr.Length - position - 1);
                    TempArray = tempstr.Split(equal, 2);
                    if (TempArray.Length > 1)
                        innerList[TempArray[0].Trim()] = TempArray[1].Trim();
                    else
                        innerList[TempArray[0].Trim()] = "";

                    position = resstr.IndexOf("\0");
                }
                TempArray = null;
                return true;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return false;
            }
        }

        private bool IsPathExists(string path)
        {
            return File.Exists(path);
        }

        #endregion

        #region Attribute relevant

        public bool GetAttrAsBoolean(string key)
        {
            try
            {
                bool res = false;
                if (innerList.ContainsKey(key))
                {
                    string str = innerList[key];
                    str = str.Trim().Substring(0, 1);
                    str = str.ToUpper();
                    if ((str != "0") && (str != "N") && (str != "F"))
                    {
                        res = true;
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return false;
            }
        }

        public Double GetAttrAsFloat(string key)
        {
            try
            {
                Double res = 0;
                if (innerList.ContainsKey(key))
                {
                    string str = innerList[key];
                    str = str.Trim();
                    res = Convert.ToDouble(str);
                }
                return res;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return 0;
            }
        }

        public int GetAttrAsInt(string key)
        {
            try
            {
                int res = -1;
                if (innerList.ContainsKey(key))
                {
                    string str = innerList[key];
                    str = str.Trim();
                    res = Convert.ToInt32(str);
                }
                return res;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                return -1;
            }
        }

        public string GetAttrAsString(string key)
        {
            if (key == null || key.Trim() == "") return "";
            if (innerList.ContainsKey(key) && innerList[key] != null)
            {
                return innerList[key];
            }
            return "";
        }

        #endregion
    }
}