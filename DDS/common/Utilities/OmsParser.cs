using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.Utilities
{
    public class OmsParser
    {
        protected string msg;
        protected int index;
        protected int len;
        protected string delimiter;
        protected byte delimiterInByte;
        protected bool isANSI;
        protected byte[] msgInByte;

        public OmsParser(string msg, string delimiter)
            : this(msg, delimiter, true)
        { }

        public OmsParser(string msg, string delimiter, bool useANSI)
        {
            this.msg = msg;
            this.delimiter = delimiter;
            this.delimiterInByte = Encoding.ASCII.GetBytes(delimiter)[0];
            len = msg.Length;
            if (len == 0) index = -1;

            if (useANSI && User32.GetSystemMetrics(User32.SM_DBCSENABLED) != 0)
                isANSI = true;
            msgInByte = Encoding.ASCII.GetBytes(msg);
        }

        public bool Remain(ref string token)
        {
            if (index < 0)
            {
                token = "";
                return false;
            }
            else
            {
                index = -1;
                token = msg.Substring(index, len - index + 1);
                return true;
            }
        }

        public bool Next(ref string token)
        {
            try
            {
                if (index < 0)
                {
                    token = "";
                    return false;
                }
                else
                {
                    int start = index;
                    while (index < len)
                    {
                        //byte b = Convert.ToByte(msg[index]);
                        byte b = msgInByte[index];
                        if (isANSI && Kernel32.IsDBCSLeadByte(b))
                        {
                            index++;
                        }
                        else
                        {
                            if (b == delimiterInByte) break;
                        }
                        index++;
                    }
                    if (index < len)
                    {
                        if (index > start)
                        {
                            if ((delimiterInByte == 10) && (msg[index - 1] == '\r'))
                                token = msg.Substring(start, index - start - 1);
                            else token = msg.Substring(start, index - start);
                        }
                        else token = "";
                        index++;
                        if (delimiter == " ")
                        {
                            while (msg[index] == ' ')
                            {
                                index++;
                            }
                        }
                    }
                    else
                    {
                        if (start >= len) token = "";
                        else token = msg.Substring(start, len - start + 1);
                        index = -1;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog("@@" + msg);
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return false;
        }
    }
}
