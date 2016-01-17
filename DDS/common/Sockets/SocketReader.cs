using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.ComponentModel;

namespace OMS.common.Sockets
{
    public class TSocketReader : IDisposable
    {
        public event EventHandler<SocketStatusEventArgs> OnStatus;
        public event EventHandler<DataBufferEventArgs> OnDataBuffer;
        public event EventHandler<SocketErrorEventArgs> OnError;

        protected NetworkStream nwReader;
        protected int bufferLen = 8192;
        protected byte[] buffer;
        protected ISynchronizeInvoke syncInvoker;
        protected bool isDisposed;

        public bool IsDisposed { get { return isDisposed; } }

        public void Dispose()
        {
            try
            {
                if (nwReader != null)
                {
                    nwReader.Close();
                    nwReader = null;
                }
            }
            catch { }
            finally
            {
                isDisposed = true;
            }
        }

        public TSocketReader(TcpClient client, int len)
        {
            isDisposed = false;
            bufferLen = len;
            if (bufferLen > 0)
                buffer = new byte[bufferLen];
            if (client != null)
                nwReader = client.GetStream();
        }

        public TSocketReader(TSocketWriter writer, int len)
        {
            isDisposed = false;
            bufferLen = len;
            if (bufferLen > 0)
                buffer = new byte[bufferLen];
            if (writer != null)
                nwReader = writer.Writer;
        }

        public TSocketReader(NetworkStream raw, int len)
        {
            isDisposed = false;
            bufferLen = len;
            if (bufferLen > 0)
                buffer = new byte[bufferLen];

            nwReader = raw;
        }

        public TSocketReader(TcpClient client)
        {
            isDisposed = false;
            buffer = new byte[bufferLen];
            if (client != null)
                nwReader = client.GetStream();
        }

        public TSocketReader(TSocketWriter writer)
        {
            isDisposed = false;
            buffer = new byte[bufferLen];
            if (writer != null)
                nwReader = writer.Writer;
        }

        public TSocketReader(NetworkStream raw)
        {
            isDisposed = false;
            buffer = new byte[bufferLen];
            nwReader = raw;
        }

        public ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }

        private void ProcessRead(IAsyncResult ar)
        {
            if (nwReader == null) return;
            if (IsDisposed) return;
            try
            {
                try
                {
                    int msglen = nwReader.EndRead(ar);

                    if (msglen > 0)
                    {
                        RaiseUpOnDataBuffer(buffer, msglen);
                    }
                    else
                    {
                        RaiseUpOnStatus(false);
                        return;
                    }
                }
                catch (Exception e1)
                {
                    RaiseUpOnError(e1);
                    if (e1 is IOException)
                    {
                        RaiseUpOnStatus(false);
                        return;
                    }
                }
                BeginRead();
            }
            catch (Exception ex)
            {
                RaiseUpOnError(ex);
            }
        }

        public void BeginRead()
        {
            if (nwReader != null && !IsDisposed)
                nwReader.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(ProcessRead), null);
        }

        private void RaiseUpOnError(Exception error)
        {
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                if (OnError != null)
                    syncInvoker.Invoke(OnError, new object[] { this, new SocketErrorEventArgs(error) });
            }
            else
            {
                if (OnError != null)
                    OnError(this, new SocketErrorEventArgs(error));
            }
        }

        private void RaiseUpOnStatus(bool connected)
        {
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                if (OnStatus != null)
                    syncInvoker.Invoke(OnStatus, new object[] { this, new SocketStatusEventArgs(connected) });
            }
            else
            {
                if (OnStatus != null)
                    OnStatus(this, new SocketStatusEventArgs(connected));
            }
        }

        private void RaiseUpOnDataBuffer(byte[] buffer, int length)
        {
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                if (OnDataBuffer != null)
                    syncInvoker.Invoke(OnDataBuffer, new object[] { this, new DataBufferEventArgs(buffer, length) });
            }
            else
            {
                if (OnDataBuffer != null)
                    OnDataBuffer(this, new DataBufferEventArgs(buffer, length));
            }
        }
    }
}