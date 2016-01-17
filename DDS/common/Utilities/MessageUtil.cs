using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.Utilities
{
    public class omsAttribute
    {
        protected int tag;
        protected string value;

        public omsAttribute(int tag, string value)
        {
            this.tag = tag;
            this.value = value;
            if (this.value == null) this.value = "";
        }

        public int Tag { get { return tag; } }

        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (this.value == null)
                    this.value = "";
            }
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|", tag, value);
        }
    }

    public class OmsItem
    {
        protected string symbol;
        protected Dictionary<string, omsAttribute> innnerList;

        public OmsItem()
        {
            innnerList = new Dictionary<string, omsAttribute>();
            symbol = "";
        }

        public string this[int id]
        {
            get { return GetValue(id); }
            set { SetValue(id, value); }
        }

        public string Symbol { get { return symbol; } set { symbol = value; } }

        public void ParseFrom(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            OmsParser parser = new OmsParser(msg, "|");
            string command = "";
            string handle = "";
            string tag = "";
            string value = "";
            if (parser.Next(ref command) && parser.Next(ref handle))
            {
                if ((command == "image") || (command == "close")) Clear();
                if ((command == "image") || (command == "update") || (command == "default"))
                {
                    int id = 0;
                    while (parser.Next(ref tag) && parser.Next(ref value))
                    {
                        if (int.TryParse(tag, out id))
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Enter(innnerList);
                            try
                            {
                                if (innnerList.ContainsKey(tag))
                                {
                                    omsAttribute attr = innnerList[tag];
                                    if ((attr.Value.Length == 0) || (command != "default"))
                                        attr.Value = value;
                                }
                                else
                                {
                                    omsAttribute attr = new omsAttribute(id, value);
                                    innnerList[tag] = attr;
                                }
                            }
                            finally
                            {
                                if (omsCommon.SyncInvoker == null)
                                    System.Threading.Monitor.Exit(innnerList);
                            }
                        }
                        else
                        {
                            if (parser.Next(ref value))
                                throw new Exception("Invalid attribute [" + tag + ":" + value + "] @" + msg);
                        }
                    }
                }
            }
        }

        public bool ContainsKey(string key)
        {
            if (key == null || key.Trim() == "") return false;
            return innnerList.ContainsKey(key);
        }

        public bool ContainsKey(int key)
        {
            return ContainsKey(key.ToString());
        }

        public void Clear()
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(innnerList);
            try
            {
                innnerList.Clear();
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(innnerList);
            }
        }

        public decimal GetDecimalValue(int id)
        {
            return GetDecimalValue(id.ToString());
        }

        public decimal GetDecimalValue(string id)
        {
            if (id == null || id.Trim() == "") return 0m;
            decimal res = 0m;
            decimal.TryParse(GetValue(id), out res);
            return res;
        }

        public string GetValue(int id)
        {
            return GetValue(id.ToString());
        }

        public string GetValue(string id)
        {
            if (id == null || id.Trim() == "") return "";
            if (innnerList.ContainsKey(id))
            {
                omsAttribute attr = innnerList[id];
                return attr.Value;
            }
            return "";
        }

        public void SetValue(int id, string value)
        {
            SetValue(id.ToString(), value);
        }

        public void SetValue(string id, string value)
        {
            if (id == null || id.Trim() == "") return;
            if (innnerList.ContainsKey(id))
            {
                innnerList[id].Value = value;
            }
            else
            {
                int tag = 0;
                if (int.TryParse(id, out tag))
                {
                    omsAttribute attr = new omsAttribute(tag, value);
                    innnerList[id] = attr;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder(symbol + "|");
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(innnerList);
            try
            {
                foreach (omsAttribute item in innnerList.Values)
                {
                    buffer.Append(item.ToString());
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(innnerList);
            }
            return buffer.ToString();
        }
    }
}
