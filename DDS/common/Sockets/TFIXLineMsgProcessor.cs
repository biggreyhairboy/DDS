using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OMS.common.Sockets
{
    public class TFIXLineMsgProcessor : TLineMsgProcessor
    {
        protected StringBuilder cache = new StringBuilder();

        public override System.ComponentModel.ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }

        public override void HandleMessage(byte[] pBuffer, int sizeOfBuffer)
        {
            if (cache == null) cache = new StringBuilder();
            cache.Append(FEncoding.GetString(pBuffer, 0, sizeOfBuffer));

            string msg = cache.ToString();
            string splitter = FEncoding.GetString(new byte[] { Convert.ToByte('\x0001') });
            string pattern = string.Format("(?<value>.*?10=.*?{0})", splitter);
            string trail = msg;
            Regex regex = new Regex(pattern);
            for (Match m = regex.Match(msg); m.Success; m = m.NextMatch())
            {
                string content = m.Groups["value"].Value;
                FireOnMsg(content);
                trail = trail.Replace(content, "");
            }

            if (trail != null && trail.Length > 0) cache = new StringBuilder(trail);
            else cache = new StringBuilder();
        }
    }
}