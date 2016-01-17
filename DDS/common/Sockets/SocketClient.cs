using System;
using System.Text;
using System.Net.Sockets;
using System.ComponentModel;
using System.Net;

namespace OMS.common.Sockets
{
    public class TSocketClient : IDisposable
    {
        #region Members

        public event EventHandler<SocketStatusEventArgs> OnSocketStatus;
        /// <summary>
        /// Fired only if <seealso cref="SocketDataMode"/> is not sdmBinary
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> OnSocketMessage;
        /// <summary>
        /// Fired only if <seealso cref="SocketDataMode"/> is sdmBinary
        /// </summary>
        public event EventHandler<BinaryReceiveEventArgs> OnSocketBinary;
        public event EventHandler<SocketErrorEventArgs> OnError;

        protected string host;
        protected int port;
        protected System.Timers.Timer reconnectTimer;
        protected Socket sock;
        protected TSocketWriter writer;
        protected TSocketReader reader;
        protected Int32 maxReConnTimes = 0;
        protected Int32 reConnTimes = 0;
        protected int retryInterval = 30000;
        protected int maxRetryInterval = 0;
        protected bool isRetryConn = false;
        protected TLineMsgProcessor msgProcessor;
        protected TSocketDataMode socketDataMode;
        protected bool isConnected = false;
        protected string remoteAddress = "";
        protected string localAddress = "";
        protected ISynchronizeInvoke syncInvoker;
        protected bool isDisposed;

        #endregion

        #region Constructors

        public TSocketClient(Socket sock, TSocketDataMode mode)
            : this(sock, mode, Encoding.Default)
        { }

        public TSocketClient(Socket sock, TSocketDataMode mode, Encoding encoding)
        {
            isDisposed = false;
            socketDataMode = mode;
            if (socketDataMode == TSocketDataMode.sdmOmsLine)
            {
                msgProcessor = new TOMSLineMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmFixLine)
            {
                msgProcessor = new TFIXLineMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmOG)
            {
                msgProcessor = new TOGMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmBinary)
            {
                msgProcessor = new BinaryProcessor();
            }
            else
            {
                msgProcessor = new TQueueLineMsgProcessor();
            }

            msgProcessor.SyncInvoker = syncInvoker;

            msgProcessor.OnMessage += new EventHandler<SocketReceiveEventArgs>(ProcessLineMessage);
            msgProcessor.OnBinary += new EventHandler<BinaryReceiveEventArgs>(ProcessBinary);
            msgProcessor.Encoding = encoding;

            try
            {
                isConnected = sock.Connected;
                writer = new TSocketWriter(sock);
                writer.SyncInvoker = syncInvoker;
                this.sock = sock;
                remoteAddress = sock.RemoteEndPoint.ToString();
                localAddress = sock.LocalEndPoint.ToString();
                writer.OnStatus += new EventHandler<SocketStatusEventArgs>(ProcessSocketStatus);
                writer.OnError += new EventHandler<SocketErrorEventArgs>(ProcessErrorMessage);
                reader = new TSocketReader(writer);
                reader.SyncInvoker = syncInvoker;
                reader.OnDataBuffer += new EventHandler<DataBufferEventArgs>(ProcessDataBuffer);
                reader.OnStatus += new EventHandler<SocketStatusEventArgs>(ProcessSocketStatus);
                reader.OnError += new EventHandler<SocketErrorEventArgs>(ProcessErrorMessage);
            }
            catch { }
        }

        public TSocketClient()
            : this(TSocketDataMode.sdmOmsLine)
        { }

        public TSocketClient(TSocketDataMode mode)
            : this(mode, System.Text.Encoding.Default)
        { }

        public TSocketClient(TSocketDataMode mode, Encoding encoding)
            : this(mode, encoding, false)
        { }

        public TSocketClient(TSocketDataMode mode, Encoding encoding, bool reconnect)
        {
            isDisposed = false;
            socketDataMode = mode;
            if (socketDataMode == TSocketDataMode.sdmOmsLine)
            {
                msgProcessor = new TOMSLineMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmFixLine)
            {
                msgProcessor = new TFIXLineMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmOG)
            {
                msgProcessor = new TOGMsgProcessor();
            }
            else if (socketDataMode == TSocketDataMode.sdmBinary)
            {
                msgProcessor = new BinaryProcessor();
            }
            else
            {
                msgProcessor = new TQueueLineMsgProcessor();
            }

            msgProcessor.SyncInvoker = syncInvoker;

            msgProcessor.OnMessage += new EventHandler<SocketReceiveEventArgs>(ProcessLineMessage);
            msgProcessor.OnBinary += new EventHandler<BinaryReceiveEventArgs>(ProcessBinary);
            msgProcessor.Encoding = encoding;

            reconnectTimer = new System.Timers.Timer();
            reconnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(Reconnect);
            reconnectTimer.Enabled = false;
            reconnectTimer.Interval = retryInterval;
            maxRetryInterval = 1800000;
            maxReConnTimes = omsCommon.MaxReconnectTimes;
            reConnTimes = 0;
            isRetryConn = reconnect;
        }

        #endregion

        #region Properties

        public string RemoteAddress { get { return remoteAddress; } }

        public string LocalAddress { get { return localAddress; } }
        /// <summary>
        /// Gets or sets the sync invoker for SocketClient, please make sure this equals to the one defined in omsCommon.cs when single threading's ON
        /// </summary>
        public ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set
            {
                syncInvoker = value;
                if (writer != null)
                    writer.SyncInvoker = syncInvoker;
                if (reader != null)
                    reader.SyncInvoker = syncInvoker;
                if (msgProcessor != null)
                    msgProcessor.SyncInvoker = syncInvoker;
            }
        }

        public bool IsConnected { get { return isConnected; } }

        public bool IsDisposed { get { return isDisposed; } }

        public TSocketDataMode SocketDataMode { get { return socketDataMode; } }

        #endregion

        #region Member of IDisposable

        public void Dispose()
        {
            try
            {
                Disconnect();

                OnSocketStatus = null;
                OnSocketMessage = null;
                OnError = null;

                if (reconnectTimer != null)
                {
                    reconnectTimer.Enabled = false;
                    reconnectTimer.Stop();
                    reconnectTimer.Dispose();
                    reconnectTimer = null;
                }
            }
            catch { }
            finally
            {
                isDisposed = true;
            }
        }

        #endregion

        #region Raise up Event

        private void RaiseUpOnMessage(string msg)
        {
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                if (OnSocketMessage != null)
                    syncInvoker.Invoke(OnSocketMessage, new object[] { this, new SocketReceiveEventArgs(msg) });
            }
            else
            {
                if (OnSocketMessage != null)
                    OnSocketMessage(this, new SocketReceiveEventArgs(msg));
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

        private void RaiseUpOnStatus(bool connnected)
        {
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                if (OnSocketStatus != null)
                    syncInvoker.Invoke(OnSocketStatus, new object[] { this, new SocketStatusEventArgs(connnected) });
            }
            else
            {
                if (OnSocketStatus != null)
                    OnSocketStatus(this, new SocketStatusEventArgs(connnected));
            }
        }

        private void RaiseUpOnBinary(byte[] buffer, int offset, int size)
        {
            if (OnSocketBinary == null) return;
            if (syncInvoker != null /*&& syncInvoker.InvokeRequired*/)
            {
                syncInvoker.Invoke(OnSocketBinary, new object[] { this, new BinaryReceiveEventArgs(buffer, offset, size) });
            }
            else
            {
                OnSocketBinary(this, new BinaryReceiveEventArgs(buffer, offset, size));
            }
        }

        #endregion

        #region Event Handlers

        private void ProcessLineMessage(object sender, SocketReceiveEventArgs e)
        {
            RaiseUpOnMessage(e.Message);
        }

        private void ProcessBinary(object sender, BinaryReceiveEventArgs e)
        {
            RaiseUpOnBinary(e.Buffer, e.Offset, e.Size);
        }

        private void ProcessSocketStatus(object sender, SocketStatusEventArgs e)
        {
            isConnected = e.Connected;
            RaiseUpOnStatus(e.Connected);
            if (!isConnected && isRetryConn)
            {
                if (reconnectTimer != null)
                {
                    reconnectTimer.Enabled = true;
                    reConnTimes = 0;
                }
            }
        }

        private void ProcessDataBuffer(object sender, DataBufferEventArgs e)
        {
            try
            {
                msgProcessor.HandleMessage(e.Buffer, e.Length);
            }
            catch (Exception ex)
            {
                RaiseUpOnError(ex);
            }
        }

        private void ProcessErrorMessage(object sender, SocketErrorEventArgs e)
        {
            if (e != null)
                RaiseUpOnError(e.LastError);
        }

        #endregion

        #region Public functions

        public void Connect(string host, int port)
        {
            try
            {
                this.host = host;
                this.port = port;
                IPAddress[] Ips = Dns.GetHostAddresses(host);
                IPAddress address = Ips[0];
                foreach (IPAddress ipa in Ips)
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = ipa;
                        break;
                    }
                }
                IPEndPoint remoteEP = new IPEndPoint(address, port);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(remoteEP);
                remoteAddress = sock.RemoteEndPoint.ToString();
                localAddress = sock.LocalEndPoint.ToString();
                if (sock.Connected)
                {
                    isConnected = true;
                    reconnectTimer.Enabled = false;
                    writer = new TSocketWriter(sock);
                    writer.SyncInvoker = syncInvoker;
                    writer.OnStatus += new EventHandler<SocketStatusEventArgs>(ProcessSocketStatus);
                    writer.OnError += new EventHandler<SocketErrorEventArgs>(ProcessErrorMessage);
                    reader = new TSocketReader(writer);
                    reader.SyncInvoker = syncInvoker;
                    reader.OnDataBuffer += new EventHandler<DataBufferEventArgs>(ProcessDataBuffer);
                    reader.OnStatus += new EventHandler<SocketStatusEventArgs>(ProcessSocketStatus);
                    reader.OnError += new EventHandler<SocketErrorEventArgs>(ProcessErrorMessage);
                    Start();
                    RaiseUpOnStatus(true);
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                reconnectTimer.Enabled = isRetryConn;
                RaiseUpOnStatus(false);
                RaiseUpOnError(ex);
            }
        }

        public void SetReconnectTimerInterval(int milliSecond)
        {
            try
            {
                if (reconnectTimer != null)
                {
                    reconnectTimer.Interval = milliSecond;
                }
            }
            catch { }
        }

        public void Disconnect()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
            if (sock != null)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock = null;
                isConnected = false;
            }

            RaiseUpOnStatus(false);
        }

        public void Start()
        {
            if (reader != null)
                reader.BeginRead();
        }

        public void SendMessage(string msg)
        {
            try
            {
                if (isConnected)
                {
                    writer.WriteMessage(msg);
                }
                else
                    RaiseUpOnStatus(false);
            }
            catch (Exception ex)
            {
                RaiseUpOnError(ex);
            }
        }

        public void SendMessage(string msg, bool withNewLine)
        {
            try
            {
                if (isConnected)
                {
                    writer.WriteMessage(msg, withNewLine);
                }
                else
                    RaiseUpOnStatus(false);
            }
            catch (Exception ex)
            {
                RaiseUpOnError(ex);
            }
        }

        public void SendBroadcastMsg(object sender, SocketBroadcastEventArgs e)
        {
            SendMessage(e.Message);
        }

        #endregion

        #region Reconnect

        protected void Reconnect(object sender, System.Timers.ElapsedEventArgs e)
        {
            reConnTimes = reConnTimes + 1;
            if (maxReConnTimes > 0)
            {
                if (reConnTimes > maxReConnTimes)
                {
                    reconnectTimer.Enabled = false;
                    OMS.common.Utilities.TLog.DefaultInstance.WriteLog(string.Format("Has reached max reconnect times: {0}, and reconnect is cancelled! ({1}:{2})", maxReConnTimes, host, port), true);
                    return;
                }
            }
            else//First 10 times interval=default value; second 10 times interval=1min; then interval add 1min every 10 times untill interval to 30min.
            {
                if (reConnTimes % 10 == 0)
                {
                    if (reConnTimes / 10 == 1)
                    {
                        reconnectTimer.Interval = 60000;//1min
                        OMS.common.Utilities.TLog.DefaultInstance.WriteLog(string.Format("Reconnect interval is changed to: {0}", reconnectTimer.Interval), true);
                    }
                    else
                    {
                        if (reconnectTimer.Interval < maxRetryInterval)//30min
                        {
                            reconnectTimer.Interval += 60000;
                            OMS.common.Utilities.TLog.DefaultInstance.WriteLog(string.Format("Reconnect interval is changed to: {0}", reconnectTimer.Interval), true);
                        }
                    }
                }
            }
            OMS.common.Utilities.TLog.DefaultInstance.WriteLog(string.Format("Trying reconnect to {0}:{1} ...", host, port), true);
            Connect(host, port);
        }

        #endregion
    }
}