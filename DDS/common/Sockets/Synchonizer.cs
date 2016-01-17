using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.Threading;
using OMS.common.Models.DataModel;
using System.Text.RegularExpressions;

namespace OMS.common.Sockets
{
    public delegate void FilterCallback(ValidateInfo info);
    public delegate void MsgFilter(ValidateInfo info, FilterCallback callback);

    public class ValidateInfo
    {
        protected string msg;
        protected bool accessible;
        protected OmsRequest request;

        public ValidateInfo(OmsRequest req)
        {
            this.request = req;
        }

        public string Message { get { return msg; } set { msg = value; } }
        public bool Accessible { get { return accessible; } set { accessible = value; } }
        public OmsRequest Request { get { return request; } }
    }

    public class RequestEventArgs : EventArgs
    {
        protected string message;
        protected OmsRequest request;

        public RequestEventArgs(string message, OmsRequest request)
        {
            this.message = message;
            this.request = request;
        }

        public string Message { get { return message; } }

        public OmsRequest Request { get { return request; } }
    }

    public class OmsRequest
    {
        protected string request;
        protected string requestID;
        protected int requestTimeout;
        protected bool done;
        protected bool success;
        protected bool obsolete;
        protected Exception lastError;
        protected EventHandler<RequestEventArgs> handler;
        protected MsgFilter validate;
        protected object data;
        protected object checkPoint;
        /// <summary>
        /// Oms request for asynchonize socket synchonization
        /// </summary>
        /// <param name="request">Request from client, always will be a command</param>
        /// <param name="requestID">Request ID, which used to identify this request</param>
        /// <param name="requestTimeout">Request timeout, in millisecond</param>
        public OmsRequest(string request, string requestID, int requestTimeout)
        {
            this.request = request;
            this.requestID = requestID;
            this.requestTimeout = requestTimeout;
        }
        /// <summary>
        /// Gets the request from client, always will be a command
        /// </summary>
        public string Request { get { return request; } }
        /// <summary>
        /// Gets the request ID, which used to identify this request
        /// </summary>
        public string RequestID { get { return requestID; } }
        /// <summary>
        /// Gets the request timeout, in millisecond
        /// </summary>
        public int RequestTimeout { get { return requestTimeout; } }
        /// <summary>
        /// Gets or sets the error for this request, should be assigned whenever an error found
        /// </summary>
        public Exception LastError { get { return lastError; } set { lastError = value; } }
        /// <summary>
        /// Gets or sets the custom process routine for this request
        /// </summary>
        public EventHandler<RequestEventArgs> Handle { get { return handler; } set { handler = value; } }
        /// <summary>
        /// Gets or sets the message filtering event
        /// </summary>
        public MsgFilter Validate { get { return validate; } set { validate = value; } }
        /// <summary>
        /// Gets or sets the custom data for a particular purpose
        /// </summary>
        public object CustomData { get { return data; } set { data = value; } }
        /// <summary>
        /// Gets or sets the done flag to identify whether or not the request process done, always will be changed in custom process routine
        /// </summary>
        public bool Done { get { return done; } set { done = value; } }
        /// <summary>
        /// Gets or sets the successful flag to identify whether or not the request is responsed successfully, always will be changed in custom process routine
        /// </summary>
        public bool Success { get { return success; } set { success = value; } }
        /// <summary>
        /// Gets or sets the value for validation check
        /// </summary>
        public object CheckPoint { get { return checkPoint; } set { checkPoint = value; } }
        /// <summary>
        /// Gets or sets the value to indicate whether or not the request is still valid
        /// </summary>
        public bool Obsolete { get { return obsolete; } set { obsolete = value; } }
    }

    public interface IOmsSynchonizer : IDisposable
    {
        event EventHandler<SocketReceiveEventArgs> OnMessage;
        event EventHandler<SocketErrorEventArgs> OnError;
        event EventHandler<SocketStatusEventArgs> OnState;

        Exception LastError { get; }
        string Host { get; }
        int Port { get; }
        bool Connect(string host, int port, int timeout);
        bool SendRequest(OmsRequest request);
        void SendRequestAsync(string msg);
    }

    public class SynchonizerBase : IOmsSynchonizer
    {
        public event EventHandler<SocketReceiveEventArgs> OnMessage;
        public event EventHandler<SocketErrorEventArgs> OnError;
        public event EventHandler<SocketStatusEventArgs> OnState;

        protected TSocketClient clientSock;
        protected AutoResetEvent lastEvent;
        protected OmsRequest lastRequest;
        protected Exception lastError;
        protected string host;
        protected int port;

        public SynchonizerBase()
        {
            clientSock = new TSocketClient(TSocketDataMode.sdmOmsLine);
            clientSock.OnError += new EventHandler<SocketErrorEventArgs>(clientSock_OnError);
            clientSock.OnSocketMessage += new EventHandler<SocketReceiveEventArgs>(clientSock_OnSocketMessage);
            clientSock.OnSocketStatus += new EventHandler<SocketStatusEventArgs>(clientSock_OnSocketStatus);
        }

        public Exception LastError { get { return lastError; } }

        public string Host { get { return host; } }

        public int Port { get { return port; } }

        private void SetEvent()
        {
            if (lastEvent != null) lastEvent.Set();
        }

        private void clientSock_OnSocketStatus(object sender, SocketStatusEventArgs e)
        {
            SetEvent();
            FireOnState(e);
        }

        private void clientSock_OnSocketMessage(object sender, SocketReceiveEventArgs e)
        {
            if (lastRequest != null)
            {
                if (!ContinuableMsg(e.Message))
                {
                    if (lastRequest.Validate != null)
                    {
                        ValidateInfo info = new ValidateInfo(lastRequest);
                        info.Message = e.Message;
                        lastRequest.Validate(info, ValidateCallback);
                    }
                    else
                    {
                        if (!lastRequest.Obsolete)
                        {
                            if (lastRequest.Handle != null)
                            {
                                if (omsCommon.SyncInvoker == null)
                                    lastRequest.Handle(sender, new RequestEventArgs(e.Message, lastRequest));
                                else omsCommon.SyncInvoker.Invoke(lastRequest.Handle, new object[] { sender, new RequestEventArgs(e.Message, lastRequest) });
                            }
                            if (lastRequest.Done)
                            {
                                SetEvent();
                            }
                        }
                    }
                }
            }
            FireOnMessage(e);
        }

        private void ValidateCallback(ValidateInfo info)
        {
            if (info.Accessible)
            {
                if (lastRequest != null)
                {
                    if (!lastRequest.Obsolete)
                    {
                        if (lastRequest.Handle != null)
                        {
                            if (omsCommon.SyncInvoker == null)
                                lastRequest.Handle(this, new RequestEventArgs(info.Message, lastRequest));
                            else omsCommon.SyncInvoker.Invoke(lastRequest.Handle, new object[] { this, new RequestEventArgs(info.Message, lastRequest) });
                        }
                        if (lastRequest.Done)
                        {
                            SetEvent();
                        }
                    }
                }
            }
        }

        protected virtual bool ContinuableMsg(string msg)
        {
            //To be done
            return false;
        }

        private void clientSock_OnError(object sender, SocketErrorEventArgs e)
        {
            TLog.DefaultInstance.WriteLog(e.LastError.ToString(), LogType.ERROR);
            if (lastRequest != null)
            {
                if (!lastRequest.Obsolete)
                {
                    lastRequest.LastError = e.LastError;
                    if (lastRequest.Handle != null)
                    {
                        if (omsCommon.SyncInvoker == null)
                            lastRequest.Handle(sender, new RequestEventArgs(e.LastError.ToString(), lastRequest));
                        else omsCommon.SyncInvoker.Invoke(lastRequest.Handle, new object[] { sender, new RequestEventArgs(e.LastError.ToString(), lastRequest) });
                    }
                    if (lastRequest.Done)
                    {
                        SetEvent();
                    }
                }
            }
            FireOnError(e);
        }

        private void FireOnMessage(SocketReceiveEventArgs e)
        {
            if (e.Message != null && e.Message.Trim() != "")
            {
                ProcessMessage(e.Message);
            }
            if (OnMessage != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnMessage(this, e);
                else omsCommon.SyncInvoker.Invoke(OnMessage, new object[] { this, e });
            }
        }

        private void FireOnError(SocketErrorEventArgs e)
        {
            if (OnError != null)
            {
                if (omsCommon.SyncInvoker == null) OnError(this, e);
                else omsCommon.SyncInvoker.Invoke(OnError, new object[] { this, e });
            }
        }

        private void FireOnState(SocketStatusEventArgs e)
        {
            if (OnState != null)
            {
                if (omsCommon.SyncInvoker == null) OnState(this, e);
                else omsCommon.SyncInvoker.Invoke(OnState, new object[] { this, e });
            }
        }

        private void ProcessMessage(string msg)
        {
            //Do nothing
        }

        #region IOmsSynchonizer Members

        public bool Connect(string host, int port, int timeout)
        {
            this.host = host;
            this.port = port;
            try
            {
                AutoResetEvent connectEvent = new AutoResetEvent(false);
                lastEvent = connectEvent;
                clientSock.Connect(host, port);
                connectEvent.WaitOne(timeout, false);
                lastEvent = null;
                return clientSock.IsConnected;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return false;
        }

        public void SendRequestAsync(string msg)
        {
            try
            {
                if (null == msg) return;
                msg = msg.Trim();
                if ("" == msg) return;

                clientSock.SendMessage(msg);
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public bool SendRequest(OmsRequest request)
        {
            try
            {
                AutoResetEvent localEvent = new AutoResetEvent(false);
                lastEvent = localEvent;
                lastRequest = request;
                request.Success = false;//reset
                clientSock.SendMessage(request.Request);
                if (!localEvent.WaitOne(request.RequestTimeout, false))
                {
                    request.LastError = new Exception("Request timeout");
                    return false;
                }
                return request.Success;
            }
            catch (Exception ex)
            {
                request.LastError = ex;
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);

                return false;
            }
            finally
            {
                lastEvent = null;
                lastError = request.LastError;
                //ResetObsolete();
            }
        }

        protected void ResetObsolete()
        {
            if (lastRequest != null) lastRequest.Obsolete = true;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (clientSock != null)
                {
                    clientSock.Dispose();
                    clientSock = null;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion
    }
}