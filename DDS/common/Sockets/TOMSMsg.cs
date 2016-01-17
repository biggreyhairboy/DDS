using System;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

namespace OMS.common.Sockets
{
    /// <summary>
    /// Summary description for TOMSMessage.
    /// </summary>
    public class TOMSMessage : Hashtable
    {
        public StringCollection header;
        public string originalMsg;
        private string delimiter = "|";
        private int headerLength = 0;

        public TOMSMessage()
        {
            header = new StringCollection();
        }

        public TOMSMessage(string sdelimiter)
        {
            delimiter = sdelimiter;
            header = new StringCollection();
        }

        /// <summary>
        /// Generate key/value pairs from the command. Make sure call SetHeadLength correctly before call this.
        /// It will clear all old pairs firstly.
        /// </summary>
        /// <param name="command"></param>
        public void CreateFromCommand(string command)
        {
            if (command.Length <= 0) return;
            originalMsg = command;
            this.Clear(); //clear old attributea;

            string tmpstr = command;
            if (headerLength > 0)
            {
                for (int i = 0; i < headerLength; i++)
                {
                    header.Add(tmpstr.Substring(0, tmpstr.IndexOf(delimiter)).Trim());
                    tmpstr = tmpstr.Substring(tmpstr.IndexOf(delimiter) + 1, tmpstr.Length
                        - (tmpstr.IndexOf(delimiter) + 1)).Trim();
                }
            }

            Int32 j = tmpstr.Length;

            while (j > 0)
            {
                try
                {
                    string Name = tmpstr.Substring(0, tmpstr.IndexOf(delimiter)).Trim();
                    tmpstr = tmpstr.Substring(tmpstr.IndexOf(delimiter) + 1, tmpstr.Length
                        - (tmpstr.IndexOf(delimiter) + 1)).Trim();
                    string Value = tmpstr.Substring(0, tmpstr.IndexOf(delimiter)).Trim();
                    tmpstr = tmpstr.Substring(tmpstr.IndexOf(delimiter) + 1, tmpstr.Length
                        - (tmpstr.IndexOf(delimiter) + 1)).Trim();

                    SetAttribute(Name, Value);
                    j = (Int32)tmpstr.Length;
                }
                catch
                {
                    break; //Invalid Name value.
                }
            }
        }

        public void SetHeaderLength(int i)
        {
            if (i > 0)
            {
                headerLength = i;
            }
        }

        public void SetAttribute(string name, string val)
        {
            if (name.Length > 0)
            {
                if (ContainsKey(name))
                {
                    Remove(name);
                }
                Add(name, val);
            }
        }

        /// <summary>
        /// Return empty string if can't find the key.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetAttribute(string name)
        {
            if (name.Length > 0)
            {
                if (ContainsKey(name))
                {
                    return (string)this[name];
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public string GetAttribute(int i)
        {
            string name = i.ToString();
            if (ContainsKey(name))
            {
                return (string)this[name];
            }
            else
            {
                return "";
            }
        }

        public void AddHeader(string head)
        {
            if (head.Length > 0)
            {
                headerLength++;
                header.Add(head);
            }
        }
    }
}