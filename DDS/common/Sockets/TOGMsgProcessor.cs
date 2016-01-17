using System;

namespace OMS.common.Sockets
{
    public class TOGMsgProcessor : TLineMsgProcessor
    {
        protected TDataBuffer FBuffer = new TDataBuffer();

        public override System.ComponentModel.ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }

        public override void HandleMessage(byte[] pBuffer, int SizeOfBuffer)
        {
            int bSize = FBuffer.Size;
            Byte[] MsgBuffer = new Byte[bSize + SizeOfBuffer];
            if (bSize > 0) FBuffer.Read(MsgBuffer, 0, bSize);
            Buffer.BlockCopy(pBuffer, 0, MsgBuffer, bSize, SizeOfBuffer);

            int idx, len;
            int ptr = 0;
            while ((idx = Array.IndexOf(MsgBuffer, Convert.ToByte(45), ptr)) >= 0)
            {
                len = idx - ptr;

                FireOnMsg(FEncoding.GetString(MsgBuffer, ptr, len + 1));

                ptr = idx + 1;
                if (ptr >= MsgBuffer.Length) break;
            }

            if (ptr < MsgBuffer.Length)
                FBuffer.Write(MsgBuffer, ptr, MsgBuffer.Length - ptr);
        }
    }
}