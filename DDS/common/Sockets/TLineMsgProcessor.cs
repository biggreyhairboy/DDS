using System;
using System.Text;
using System.ComponentModel;

namespace OMS.common.Sockets
{
    public abstract class TLineMsgProcessor
    {
        public event EventHandler<SocketReceiveEventArgs> OnMessage;
        public event EventHandler<BinaryReceiveEventArgs> OnBinary;

        protected Encoding FEncoding;
        protected ISynchronizeInvoke syncInvoker;

        public Encoding Encoding
        {
            get { return FEncoding; }
            set { FEncoding = value; }
        }

        public abstract ISynchronizeInvoke SyncInvoker { get;set;}

        public abstract void HandleMessage(byte[] pBuffer, int sizeOfBuffer);

        public void FireOnMsg(string msg)
        {
            if (OnMessage == null) return;
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                syncInvoker.Invoke(OnMessage, new object[] { this, new SocketReceiveEventArgs(msg) });
            }
            else
            {
                OnMessage(this, new SocketReceiveEventArgs(msg));
            }
        }

        public void FireOnBinary(byte[] buffer, int offset, int size)
        {
            if (OnBinary == null) return;
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                syncInvoker.Invoke(OnBinary, new object[] { this, new BinaryReceiveEventArgs(buffer, offset, size) });
            }
            else
            {
                OnBinary(this, new BinaryReceiveEventArgs(buffer, offset, size));
            }
        }
    }
}