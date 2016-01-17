using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.Sockets
{
    public class BinaryProcessor : TLineMsgProcessor
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
            while ((idx = Array.IndexOf(MsgBuffer, Convert.ToByte(10), ptr)) >= 0)
            {
                len = idx - ptr;
                if (len > 0 && MsgBuffer[idx - 1] == 13) len--;

                FireOnBinary(MsgBuffer, ptr, len);

                ptr = idx + 1;
                if (ptr >= MsgBuffer.Length) break;
            }

            if (ptr < MsgBuffer.Length)
                FBuffer.Write(MsgBuffer, ptr, MsgBuffer.Length - ptr);
        }
    }
}
