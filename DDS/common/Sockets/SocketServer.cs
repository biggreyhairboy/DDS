using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace OMS.common.Sockets
{
    public class TSocketServer : IDisposable
    {
        protected TSocketDataMode FSocketDataMode;
        protected string host;
        protected int port;
        protected Socket serverSock;
        protected Dictionary<string, TSocketClient> clientList;
        protected bool isDisposed;

        protected event EventHandler<SocketBroadcastEventArgs> OnBroadCastMsg = null;
        public event EventHandler<SocketClientConnectEventArgs> OnClientConnect = null;
        public event EventHandler<SocketClientConnectEventArgs> OnClientDisconnect = null;
        public event EventHandler<SocketServerReceiveEventArgs> OnClientMessage = null;
        public event EventHandler<SocketErrorEventArgs> OnError = null;
        public event EventHandler<SocketStatusEventArgs> OnServerShutdown;

        public TSocketServer(string ip, int port)
            : this(ip, port, TSocketDataMode.sdmOmsLine)
        { }

        public TSocketServer(string ip, int port, TSocketDataMode mode)
        {
            isDisposed = false;
            FSocketDataMode = mode;
            this.host = ip;
            this.port = port;
            clientList = new Dictionary<string, TSocketClient>();
        }

        private void FireOnServerShutdown(SocketStatusEventArgs e)
        {
            if (OnServerShutdown != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnServerShutdown(this, e);
                else omsCommon.SyncInvoker.Invoke(OnServerShutdown, new object[] { this, e });
            }
        }

        private void FireOnError(SocketErrorEventArgs e)
        {
            if (OnError != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnError(this, e);
                else omsCommon.SyncInvoker.Invoke(OnError, new object[] { this, e });
            }
        }

        private void FireOnClientMessage(SocketServerReceiveEventArgs e)
        {
            if (OnClientMessage != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnClientMessage(this, e);
                else omsCommon.SyncInvoker.Invoke(OnClientMessage, new object[] { this, e });
            }
        }

        private void FireOnClientDisconnect(SocketClientConnectEventArgs e)
        {
            if (OnClientDisconnect != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnClientDisconnect(this, e);
                else omsCommon.SyncInvoker.Invoke(OnClientDisconnect, new object[] { this, e });
            }
        }

        private void FireOnBroadCastMsg(SocketBroadcastEventArgs e)
        {
            if (OnBroadCastMsg != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnBroadCastMsg(this, e);
                else omsCommon.SyncInvoker.Invoke(OnBroadCastMsg, new object[] { this, e });
            }
        }

        private void FireOnClientConnect(SocketClientConnectEventArgs e)
        {
            if (OnClientConnect != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnClientConnect(this, e);
                else omsCommon.SyncInvoker.Invoke(OnClientConnect, new object[] { this, e });
            }
        }

        public string Address { get { return string.Format("{0}:{1}", host, port); } }

        public bool IsDisposed { get { return isDisposed; } }

        public void Dispose()
        {
            try
            {
                if (serverSock != null)
                {
                    serverSock.Shutdown(SocketShutdown.Both);
                    serverSock = null;
                    FireOnServerShutdown(new SocketStatusEventArgs(false));
                }
                OnBroadCastMsg = null;
                OnClientConnect = null;
                OnClientDisconnect = null;
                OnClientMessage = null;
                OnError = null;

                if (clientList != null)
                {
                    foreach (TSocketClient client in clientList.Values)
                    {
                        client.Dispose();
                    }
                    clientList.Clear();
                    clientList = null;
                }
            }
            catch { }
            finally
            {
                isDisposed = true;
            }
        }

        public void Start()
        {
            try
            {
                IPAddress[] Ips = Dns.GetHostAddresses(host);
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
                serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSock.Bind(localEP);
                serverSock.Listen(1000);
                serverSock.BeginAccept(new System.AsyncCallback(AcceptClientSocket), true);
            }
            catch (Exception ex)
            {
                FireOnError(new SocketErrorEventArgs(ex));
            }
        }

        private void AcceptClientSocket(IAsyncResult ar)
        {
            try
            {
                try
                {
                    if (serverSock == null) return;

                    Socket client = serverSock.EndAccept(ar);
                    StoreAcceptSocket(client);
                    serverSock.BeginAccept(new System.AsyncCallback(AcceptClientSocket), true);
                }
                catch (Exception e)
                {
                    FireOnError(new SocketErrorEventArgs(e));
                    serverSock.BeginAccept(new System.AsyncCallback(AcceptClientSocket), true);
                }
            }
            catch (Exception ex)
            {
                FireOnError(new SocketErrorEventArgs(ex));
            }
        }

        private void StoreAcceptSocket(Socket sock)
        {
            TSocketClient client = new TSocketClient(sock, FSocketDataMode);
            client.OnSocketMessage += new EventHandler<SocketReceiveEventArgs>(HandleClientMsg);
            client.OnSocketStatus += new EventHandler<SocketStatusEventArgs>(HandleClientStatus);
            client.OnError += new EventHandler<SocketErrorEventArgs>(HandleClientError);
            clientList[client.RemoteAddress] = client;
            FireOnClientConnect(new SocketClientConnectEventArgs(true, client.RemoteAddress));
            client.Start();
        }

        private void HandleClientMsg(object sender, SocketReceiveEventArgs e)
        {
            FireOnClientMessage(new SocketServerReceiveEventArgs(sender as TSocketClient, e.Message));
        }

        private void HandleClientError(object sender, SocketErrorEventArgs e)
        {
            FireOnError(e);
        }

        private void HandleClientStatus(object sender, SocketStatusEventArgs e)
        {
            if (sender is TSocketClient)
            {
                TSocketClient aSocketClient = (TSocketClient)sender;
                if (e.Connected)
                    OnBroadCastMsg += new EventHandler<SocketBroadcastEventArgs>(aSocketClient.SendBroadcastMsg);
                else
                {
                    OnBroadCastMsg -= new EventHandler<SocketBroadcastEventArgs>(aSocketClient.SendBroadcastMsg);
                    FireOnClientDisconnect(new SocketClientConnectEventArgs(false, aSocketClient.RemoteAddress));
                }
            }
        }

        public void BroadCastMessage(SocketBroadcastEventArgs e)
        {
            try
            {
                FireOnBroadCastMsg(e);
            }
            catch (Exception ex)
            {
                FireOnError(new SocketErrorEventArgs(ex));
            }
        }

        public void SendClientMsg(TSocketClient client, string msg)
        {
            try
            {
                if (client == null)
                {
                    FireOnError(new SocketErrorEventArgs(new ArgumentNullException("client")));
                }
                else if (client.IsConnected)
                {
                    client.SendMessage(msg);
                    OMS.common.Utilities.TLog.DefaultInstance.WriteLog(msg);
                }
            }
            catch (Exception ex)
            {
                FireOnError(new SocketErrorEventArgs(ex));
            }
        }

        public void SendClientMsg(string clientID, string Msg)
        {
            SendClientMsg(clientID, Msg, true);
        }

        public void SendClientMsg(string clientID, string msg, bool withNewLine)
        {
            try
            {
                if (!clientList.ContainsKey(clientID))
                {
                    FireOnError(new SocketErrorEventArgs(new ArgumentException("clientID")));
                }
                else
                {
                    TSocketClient aClient = clientList[clientID] as TSocketClient;
                    if (aClient == null)
                    {
                        FireOnError(new SocketErrorEventArgs(new ArgumentNullException(clientID)));
                        return;
                    }
                    if (aClient.IsConnected)
                    {
                        aClient.SendMessage(msg, withNewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                FireOnError(new SocketErrorEventArgs(ex));
            }
        }
    }
}