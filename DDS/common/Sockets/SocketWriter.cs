using System;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;
using System.Text;

namespace OMS.common.Sockets
{
    public class TSocketWriter : IDisposable
    {
        public event EventHandler<SocketStatusEventArgs> OnStatus;
        public event EventHandler<SocketErrorEventArgs> OnError;

        protected NetworkStream nwWriter;
        protected ISynchronizeInvoke syncInvoker;
        protected bool isDisposed;

        public NetworkStream Writer { get { return nwWriter; } }

        public bool IsDisposed { get { return isDisposed; } }

        public ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }

        public void Dispose()
        {
            try
            {
                if (nwWriter != null)
                {
                    nwWriter.Close();
                    nwWriter = null;
                }
            }
            catch { }
            finally
            {
                isDisposed = true;
            }
        }

        public TSocketWriter(TcpClient client)
        {
            isDisposed = false;
            if (client != null)
                nwWriter = client.GetStream();
        }

        public TSocketWriter(Socket sock)
        {
            isDisposed = false;
            if (sock != null)
                nwWriter = new NetworkStream(sock);
        }

        public void WriteMessage(string msg, bool withNewLine)
        {
            if (nwWriter == null) return;
            if (isDisposed) return;

            try
            {
                if (withNewLine) msg += "\r\n";
                byte[] bytes = Encoding.Default.GetBytes(msg);

                nwWriter.Write(bytes, 0, bytes.Length);
                nwWriter.Flush();
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    RaiseUpOnStatus(false);
                }
                RaiseUpOnError(ex);
            }
        }

        public void WriteMessage(string msg)
        {
            if (nwWriter == null) return;
            if (isDisposed) return;

            try
            {
                msg += "\r\n";
                byte[] bytes = Encoding.Default.GetBytes(msg);
                lock (nwWriter)//todo: Client-Side is OK, but bad performance as Server-Side
                {
                    nwWriter.Write(bytes, 0, bytes.Length);
                    nwWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    RaiseUpOnStatus(false);
                RaiseUpOnError(ex);
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
    }
}