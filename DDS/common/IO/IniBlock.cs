using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.IO
{
    public class IniBlock
    {
        protected string section;
        protected Dictionary<string, string> innerList;

        public IniBlock()
        {
            innerList = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string this[string key]
        {
            get
            {
                if (ContainsKey(key)) return innerList[key];
                return "";
            }
        }

        public string Section { get { return section; } set { section = value; } }

        public bool IsValidBlock { get { return (section != null && section.Trim() != ""); } }

        public Dictionary<string, string> KeyValuePair { get { return innerList; } }

        public void Add(string key, string value)
        {
            innerList[key] = value;
        }

        public bool ContainsKey(string key)
        {
            if (innerList == null) return false;
            if (key == null) return false;
            return innerList.ContainsKey(key);
        }
    }
}