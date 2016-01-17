using System;
using System.Text;
using System.Collections.Generic;

namespace OMS.common.Sockets
{
    public class TQueueLineMsgProcessor : TLineMsgProcessor
    {
        protected Queue<byte> FQueue = new Queue<byte>();

        public override System.ComponentModel.ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }

        public string BytetoString()
        {
            int bytesize = FQueue.Count;
            if (bytesize < 1) return string.Empty;

            Byte[] bufferByte = new Byte[bytesize];
            bufferByte = FQueue.ToArray();

            if (FEncoding != null)
                return FEncoding.GetString(bufferByte, 0, bytesize);

            return Encoding.Default.GetString(bufferByte, 0, bytesize);
        }

        public override void HandleMessage(byte[] pBuffer, int sizeOfBuffer)
        {
            string msg = string.Empty;

            for (int i = 0; i < sizeOfBuffer; i++)
            {
                if ((pBuffer[i] == 13) || (pBuffer[i] == 10))
                {
                    msg = BytetoString();
                    FireOnMsg(msg);
                    FQueue.Clear();
                }
                else
                {
                    FQueue.Enqueue(pBuffer[i]);
                }
            }
        }
    }
}