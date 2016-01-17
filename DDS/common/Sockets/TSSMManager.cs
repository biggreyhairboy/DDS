using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OMS.common.Sockets;
using OMS.common.Utilities;

namespace OMS.common.Sockets
{
    public enum SSMBehaviorType
    {
        Login,
        Logout,
        Verify,
        CheckPwd,
        ChangePwd
    }

    public enum TLoginType
    {
        User = 0,
        Account = 1,
        Undefined
    }

    public class DisconnetEventArgs : EventArgs
    {
        protected string sessionID;

        public DisconnetEventArgs(string aSessionID)
        {
            this.sessionID = aSessionID;
        }

        public string SessionID { get { return sessionID; } }
    }

    public class LogEventArgs : EventArgs
    {
        protected string logMsg;

        public LogEventArgs(string msg)
        {
            this.logMsg = msg;
        }

        public string LogMessage { get { return logMsg; } }
    }

    public class TBasicResult
    {
        protected bool FSuccess;
        protected int FErrorCode;
        protected string FErrorMsg;
        protected int clientType;

        public TBasicResult()
        {
            FSuccess = false;
            FErrorCode = -1;
            FErrorMsg = "";
            clientType = -1;
        }

        public int ClientType { get { return clientType; } set { clientType = value; } }

        public bool Success
        {
            get { return FSuccess; }
            set { FSuccess = value; }
        }

        public int ErrorCode
        {
            get { return FErrorCode; }
            set { FErrorCode = value; }
        }

        public string ErrorMessage
        {
            get { return FErrorMsg; }
            set { FErrorMsg = value; }
        }

        public virtual void Clear()
        {
            FSuccess = false;
            FErrorCode = -1;
            FErrorMsg = "";
        }
    }

    public class TLoginResult : TBasicResult
    {
        protected string loginID;
        protected string applicationID;
        protected string sessionID;
        protected string sessionKey;
        protected string userCode;
        protected string expireDate;
        protected string serverDate;
        protected string clientIP;
        protected string sessionSymbol;
        protected int heartbeatInterval;
        protected List<string> _entitlements;

        public TLoginResult()
            : base()
        {
            sessionID = "";
            sessionKey = "";
            userCode = "";
            expireDate = "";
            serverDate = "";
            clientIP = "";
            sessionSymbol = "";
            heartbeatInterval = 0;
            _entitlements = new List<string>();
        }

        public string SessionID
        {
            get { return sessionID; }
            set { sessionID = value; }
        }

        public string SessionKey
        {
            get { return sessionKey; }
            set { sessionKey = value; }
        }

        public string UserCode { get { return userCode; } set { userCode = value; } }

        public string ApplicationID
        {
            get { return applicationID; }
            set { applicationID = value; }
        }

        public string LoginID
        {
            get { return loginID; }
            set { loginID = value; }
        }

        public string ExpireDate
        {
            get { return expireDate; }
            set { expireDate = value; }
        }

        public string ServerDate
        {
            get { return serverDate; }
            set { serverDate = value; }
        }

        public string ClientIP
        {
            get { return clientIP; }
            set { clientIP = value; }
        }

        public string SessionSymbol
        {
            get { return sessionSymbol; }
            set { sessionSymbol = value; }
        }

        public int HeartbeatInterval
        {
            get { return heartbeatInterval; }
            set { heartbeatInterval = value; }
        }

        public List<string> Entitlements
        {
            get { return _entitlements; }
            set { _entitlements = value; }
        }

        public override void Clear()
        {
            sessionID = "";
            expireDate = "";
            serverDate = "";
            clientIP = "";
            sessionSymbol = "";
            heartbeatInterval = 0;
            if (_entitlements != null)
                _entitlements = null;
            base.Clear();
        }
    }

    public class TDisconnectResult : TBasicResult
    {
        protected string sessionID;

        public TDisconnectResult()
            : base()
        {
            sessionID = "";
        }

        public string SessionID
        {
            get { return sessionID; }
            set { sessionID = value; }
        }

        public override void Clear()
        {
            base.Clear();
            sessionID = "";
        }
    }

    public class SSMManager : IDisposable
    {
        public event EventHandler<DisconnetEventArgs> OnDisconnectSSM;

        private TSocketClient client;
        private string appID;
        private string appVersion;
        private string clientHost;
        private string Fhost;
        private int Fport;
        private int FtimeOut;
        private Dictionary<string, AutoResetEvent> eventHash;
        private Dictionary<string, TBasicResult> replyHash;
        private TOMSMessage omsMsg;
        private Object syncRoot = new Object();

        public SSMManager(string host, int port, int timeOut)
        {
            Fhost = host;
            Fport = port;
            FtimeOut = timeOut;
            clientHost = "0.0.0.0";
            omsMsg = new TOMSMessage("|");
            eventHash = new Dictionary<string, AutoResetEvent>();
            replyHash = new Dictionary<string, TBasicResult>();
            client = new TSocketClient();
            client.OnSocketMessage += new EventHandler<SocketReceiveEventArgs>(client_OnSocketMessage);
            client.OnError += new EventHandler<SocketErrorEventArgs>(client_OnError);
            client.OnSocketStatus += new EventHandler<SocketStatusEventArgs>(client_OnSocketStatus);
        }

        public string ApplicationID { get { return appID; } set { appID = value; } }

        public string ApplicationVersion { get { return appVersion; } set { appVersion = value; } }

        public string ClientHost { get { return clientHost; } set { clientHost = value; } }

        private void WriteLogMsg(string msg)
        {
            TLog.DefaultInstance.WriteMaskLog(msg);
        }

        private void client_OnSocketStatus(object sender, SocketStatusEventArgs e)
        {
            string status = e.Connected ? "Connected" : "Disconnected";
            WriteLogMsg("SSM Socket Stauts: " + status);
        }

        private void client_OnError(object sender, SocketErrorEventArgs e)
        {
            WriteLogMsg("Error in SSM Socket: " + e.LastError.ToString());
        }

        private void client_OnSocketMessage(object sender, SocketReceiveEventArgs e)
        {
            WriteLogMsg(e.Message);
            string msgType = e.Message.Split('|')[0];
            if (msgType != null && msgType.Length > 0)
            {
                msgType = msgType.ToUpper();
            }
            switch (msgType)
            {
                case "SESSION":
                    ProcessSessionMsg(e.Message);
                    break;
                case "CHECKPWD":
                    ProcessCheckPwdMsg(e.Message);
                    break;
                case "ERROR":
                    ProcessErrorMsg(e.Message);
                    break;
                case "PASSWORD":
                    ProcessChangePwdMsg(e.Message);
                    break;
                case "LOGOUT":
                    ProcessLogoutMsg(e.Message);
                    break;
                case "DISCONNECT":
                    ProcessDisconnectMsg(e.Message);
                    break;
                default:
                    break;
            }
        }

        private void ProcessErrorMsg(string errorMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(errorMsg);
            string errorCode = omsMsg.GetAttribute(omsConst.OMS_ERRORCODE);
            string key = omsMsg.GetAttribute(13).ToUpper();
            if(key==null||key.Trim()=="")
                key = omsMsg.GetAttribute(74).ToUpper();
            int code = -1;
            int.TryParse(errorCode, out code);
            switch (code)
            {
                case OmsErrorConst.OMS_SSMERR_PWDCHGFAILED:
                    key += SSMBehaviorType.ChangePwd;
                    break;
                case OmsErrorConst.OMS_SSMERR_PWDCHECKFAILED:
                    key += SSMBehaviorType.CheckPwd;
                    break;
                case OmsErrorConst.OMS_SSMERR_VERIFYFAILED:
                case OmsErrorConst.OMS_SSMERR_VERIFYTIMEOUT:
                    key += SSMBehaviorType.Verify;
                    break;
                case OmsErrorConst.OMS_SSMERR_LOGOUTFAILED:
                    key += SSMBehaviorType.Logout;
                    break;
                case OmsErrorConst.OMS_SSMERR_LOGINFAILED:
                case OmsErrorConst.OMS_SSMERR_PWDEXPIRED:
                case OmsErrorConst.OMS_SSMERR_USERSUSPENDED:
                case OmsErrorConst.OMS_SSMERR_ACCTEXPIRED:
                case OmsErrorConst.OMS_SSMERR_NOCERTIFICATE:
                case OmsErrorConst.OMS_SSMERR_NOTENOUGHTLICENSE:
                case OmsErrorConst.OMS_SSMERR_NOAPPLICATIONID:
                case OmsErrorConst.OMS_SSMERR_NOTENOUGHTUSER:
                case OmsErrorConst.OMS_SSMERR_NOTENOUGHTACCT:
                    key += SSMBehaviorType.Login;
                    break;
                default:
                    break;
            }
            if (!replyHash.ContainsKey(key)) return;
            TBasicResult result = replyHash[key];
            result.Success = false;
            result.ErrorCode = code;
            result.ErrorMessage = omsMsg.GetAttribute(25);
            if (omsMsg.ContainsKey(22))
            {
                int clientType = -1;
                if (!int.TryParse(omsMsg.GetAttribute(22), out clientType))
                    clientType = -1;
                result.ClientType = clientType;
            }

            if (eventHash.ContainsKey(key))
            {
                AutoResetEvent resetEvent = eventHash[key];
                if (resetEvent != null)
                {
                    resetEvent.Set();
                }
            }
        }

        private void ProcessSessionMsg(string sessionMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(sessionMsg);

            string[] strArr = omsMsg.GetAttribute(13).Split('@');
            string loginId = strArr[0].ToUpper();

            string key = "";
            if (replyHash.ContainsKey(loginId + SSMBehaviorType.Login))
                key = loginId + SSMBehaviorType.Login;
            else if (replyHash.ContainsKey(loginId + SSMBehaviorType.Verify))
                key = loginId + SSMBehaviorType.Verify;
            else
                return;

            TLoginResult res = replyHash[key] as TLoginResult;
            if (res == null) return;
            res.Success = true;
            res.LoginID = loginId;
            res.ApplicationID = appID;
            res.SessionID = omsMsg.GetAttribute(0);
            res.SessionKey = omsMsg.GetAttribute(191);
            res.UserCode = omsMsg.GetAttribute(13);
            res.ExpireDate = omsMsg.GetAttribute(29);
            res.ServerDate = omsMsg.GetAttribute(134);
            res.ClientIP = omsMsg.GetAttribute(100);
            res.SessionSymbol = omsMsg.GetAttribute(101);
            int ssmHeartbeat = 0;
            if (int.TryParse(omsMsg.GetAttribute(102), out ssmHeartbeat))
                res.HeartbeatInterval = ssmHeartbeat;

            int clientType = 0;
            int.TryParse(omsMsg.GetAttribute(22), out clientType);
            res.ClientType = clientType;

            List<string> entitlement = new List<string>();
            int i = 200;
            while (omsMsg.GetAttribute(i) != "")
            {
                entitlement.Add(omsMsg.GetAttribute(i).ToUpper());
                i++;
            }
            if (entitlement.Count != 0)
            {
                res.Entitlements = entitlement;
            }

            if (eventHash.ContainsKey(key))
            {
                AutoResetEvent resetEvent = eventHash[key];
                if (resetEvent != null)
                {
                    resetEvent.Set();
                }
            }
        }

        private void ProcessCheckPwdMsg(string checkPwdMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(checkPwdMsg);

            string loginId = omsMsg.GetAttribute(13).ToUpper();
            string key = loginId + SSMBehaviorType.CheckPwd;
            if (!replyHash.ContainsKey(key)) return;
            TBasicResult res = replyHash[key];
            if (res == null) return;
            res.Success = true;

            if (eventHash.ContainsKey(key))
            {
                AutoResetEvent resetEvent = eventHash[key];
                if (resetEvent != null)
                {
                    resetEvent.Set();
                }
            }
        }

        private void ProcessChangePwdMsg(string changePwdMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(changePwdMsg);

            string loginId = omsMsg.GetAttribute(13).ToUpper();
            string key = loginId + SSMBehaviorType.ChangePwd;
            if (!replyHash.ContainsKey(key)) return;
            TBasicResult res = replyHash[key];
            if (res == null) return;
            res.Success = true;

            if (eventHash.ContainsKey(key))
            {
                AutoResetEvent resetEvent = eventHash[key];
                if (resetEvent != null)
                {
                    resetEvent.Set();
                }
            }
        }

        private void ProcessDisconnectMsg(string disconnectMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(disconnectMsg);

            string[] strArr = omsMsg.GetAttribute(13).Split('@');
            string loginId = strArr[0].ToUpper();
            string disconnectCode = omsMsg.GetAttribute(39);
            string sessionID = omsMsg.GetAttribute(0);
            if (disconnectCode != "53")
            {
                //kicked out case
                if (OnDisconnectSSM != null)
                    OnDisconnectSSM(this, new DisconnetEventArgs(sessionID));
            }
        }

        private void ProcessLogoutMsg(string logoutMsg)
        {
            omsMsg.SetHeaderLength(1);
            omsMsg.CreateFromCommand(logoutMsg);

            string loginID = omsMsg.GetAttribute(13).ToUpper();
            string sessionID = omsMsg.GetAttribute(0);
            string key = loginID + SSMBehaviorType.Logout;

            if (!replyHash.ContainsKey(key)) return;
            TDisconnectResult res = replyHash[key] as TDisconnectResult;
            if (res == null) return;
            res.Success = true;
            res.SessionID = sessionID;

            if (eventHash.ContainsKey(key))
            {
                AutoResetEvent resetEvent = eventHash[key];
                if (resetEvent != null)
                {
                    resetEvent.Set();
                }
            }
        }

        public bool Login(string loginId, string password, TLoginType type, string deviceID, ref TLoginResult result)
        {
            loginId = loginId.ToUpper();
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.Login))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate login rejected";
                    WriteLogMsg("Duplicate Login: " + loginId);
                    return false;
                }
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.Verify))
                {
                    result.Success = false;
                    result.ErrorMessage = "Waiting verify";
                    WriteLogMsg("Waiting verify: " + loginId);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[loginId + SSMBehaviorType.Login] = localEvent;
                replyHash[loginId + SSMBehaviorType.Login] = result;
            }

            string outMsg = null;
            outMsg = String.Format("login|13|{0}|21|{1}|", loginId, password);
            if (deviceID != null && deviceID.Trim() != "")
            {
                outMsg = String.Format("{0}{1}|{2}|", outMsg, "157", deviceID);
            }
            if (appID != null && appID.Trim() != "")
            {
                outMsg = string.Format("{0}{1}|{2}|", outMsg, "99", appID);
            }
            if (appVersion != null && appVersion.Trim() != "")
            {
                outMsg = string.Format("{0}{1}|{2}|", outMsg, "110", appVersion);
            }
            outMsg = string.Format("{0}{1}|{2}|74|{3}|", outMsg, "115", clientHost, loginId);

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(loginId + SSMBehaviorType.Login);
                replyHash.Remove(loginId + SSMBehaviorType.Login);
                result.Success = false;
                result.ErrorCode = OmsErrorConst.OMS_SSMERR_NOCONNECTION;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(outMsg);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request timeout";
                WriteLogMsg(result.ErrorMessage + ": " + outMsg);
            }
            eventHash.Remove(loginId + SSMBehaviorType.Login);
            replyHash.Remove(loginId + SSMBehaviorType.Login);
            return result.Success;
        }
        /// <summary>
        /// Another Logout function for SSM logout with synchorize waiting
        /// </summary>
        /// <param name="loginID">ID for SSM logout</param>
        /// <param name="sessionID">Session ID for SSM logout</param>
        public void Logout(string loginID, string sessionID)
        {
            if (loginID == null || loginID.Trim() == "") return;
            if (sessionID == null || sessionID.Trim() == "") return;
            loginID = loginID.ToUpper();
            string cmd = string.Format("logout|0|{0}|13|{1}|", sessionID, loginID);
            if (!client.IsConnected) client.Connect(Fhost, Fport);
            if (!client.IsConnected) return;
            client.SendMessage(cmd);
        }

        public bool Logout(string loginId, string sessionID, ref TDisconnectResult result)
        {
            loginId = loginId.ToUpper();
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.Logout))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate logout rejected";
                    WriteLogMsg("Duplicate logout: " + loginId);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[loginId + SSMBehaviorType.Logout] = localEvent;
                replyHash[loginId + SSMBehaviorType.Logout] = result;
            }

            string outMsg = null;
            //add loginId as parameter for to process logout message
            outMsg = String.Format("logout|0|{0}|13|{1}|", sessionID, loginId);

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(loginId + SSMBehaviorType.Logout);
                replyHash.Remove(loginId + SSMBehaviorType.Logout);
                result.Success = false;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(outMsg);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request time out";
                WriteLogMsg(result.ErrorMessage + ": " + outMsg);
            }
            eventHash.Remove(loginId + SSMBehaviorType.Logout);
            replyHash.Remove(loginId + SSMBehaviorType.Logout);
            return result.Success;
        }

        public bool Verify(string loginId, string sessionID, ref TLoginResult result)
        {
            loginId = loginId.ToUpper();
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.Login))
                {
                    result.Success = false;
                    result.ErrorMessage = "Waiting Login rejected";
                    WriteLogMsg("Waiting Login : " + loginId);
                    return false;
                }
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.Verify))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate verify rejected";
                    WriteLogMsg("Duplicate verify: " + loginId);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[loginId + SSMBehaviorType.Verify] = localEvent;
                replyHash[loginId + SSMBehaviorType.Verify] = result;
            }

            string outMsg = null;
            outMsg = String.Format("verify|0|{0}|74|{1}|", sessionID, loginId);

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(loginId + SSMBehaviorType.Verify);
                replyHash.Remove(loginId + SSMBehaviorType.Verify);
                result.Success = false;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(outMsg);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request time out";
                WriteLogMsg(result.ErrorMessage + ": " + outMsg);
            }
            eventHash.Remove(loginId + SSMBehaviorType.Verify);
            replyHash.Remove(loginId + SSMBehaviorType.Verify);
            return result.Success;
        }

        public bool CheckPassword(string loginId, string password, ref TBasicResult result)
        {
            loginId = loginId.ToUpper();
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.CheckPwd))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate check password rejected";
                    WriteLogMsg("Duplicate check password: " + loginId);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[loginId + SSMBehaviorType.CheckPwd] = localEvent;
                replyHash[loginId + SSMBehaviorType.CheckPwd] = result;
            }

            string outMsg = null;
            outMsg = String.Format("Checkpwd|13|{0}|21|{1}|74|{2}|", loginId, password, loginId);

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(loginId + SSMBehaviorType.CheckPwd);
                replyHash.Remove(loginId + SSMBehaviorType.CheckPwd);
                result.Success = false;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(outMsg);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request time out";
                WriteLogMsg(result.ErrorMessage + ": " + outMsg);
            }
            eventHash.Remove(loginId + SSMBehaviorType.CheckPwd);
            replyHash.Remove(loginId + SSMBehaviorType.CheckPwd);
            return result.Success;
        }

        public bool ChangePassword(string loginId, string oldPwd, string newPwd, TLoginType TLoginType, ref TBasicResult result)
        {
            loginId = loginId.ToUpper();
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(loginId + SSMBehaviorType.ChangePwd))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate change password rejected";
                    WriteLogMsg("Duplicate change password: " + loginId);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[loginId + SSMBehaviorType.ChangePwd] = localEvent;
                replyHash[loginId + SSMBehaviorType.ChangePwd] = result;
            }

            string outMsg = null;
            int _loginType = 0;
            if (TLoginType == TLoginType.User)
                _loginType = 0;
            else if (TLoginType == TLoginType.Account)
                _loginType = 1;
            else
                _loginType = -1; //No need to specify the login type.
            if (-1 != _loginType)
            {
                outMsg = String.Format("password|13|{0}|21|{1}|25|{2}|22|{3}|74|{0}|", loginId, oldPwd, newPwd, _loginType);
            }
            else
            {
                outMsg = String.Format("password|13|{0}|21|{1}|25|{2}|74|{0}|", loginId, oldPwd, newPwd);
            }

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(loginId + SSMBehaviorType.ChangePwd);
                replyHash.Remove(loginId + SSMBehaviorType.ChangePwd);
                result.Success = false;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(outMsg);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request time out";
                WriteLogMsg(result.ErrorMessage + ": " + outMsg);
            }
            eventHash.Remove(loginId + SSMBehaviorType.ChangePwd);
            replyHash.Remove(loginId + SSMBehaviorType.ChangePwd);
            return result.Success;
        }

        private bool SSMOperate(string command, string operatationKey, ref TBasicResult result)
        {
            AutoResetEvent localEvent = null;
            lock (syncRoot)
            {
                if (eventHash.ContainsKey(operatationKey))
                {
                    result.Success = false;
                    result.ErrorMessage = "Duplicate operatation rejected";
                    WriteLogMsg("Duplicate operatation: " + command);
                    return false;
                }

                localEvent = new AutoResetEvent(false);
                eventHash[operatationKey] = localEvent;
                replyHash[operatationKey] = result;
            }

            if (!client.IsConnected)
            {
                client.Connect(Fhost, Fport);
            }
            if (!client.IsConnected)
            {
                eventHash.Remove(operatationKey);
                replyHash.Remove(operatationKey);
                result.Success = false;
                result.ErrorMessage = "Can't connect to SSM Svr";
                return false;
            }

            client.SendMessage(command);
            if (!localEvent.WaitOne(FtimeOut, false))
            {
                result.Success = false;
                result.ErrorMessage = "Request time out";
                WriteLogMsg(result.ErrorMessage + ": " + command);
            }
            eventHash.Remove(operatationKey);
            replyHash.Remove(operatationKey);
            return result.Success;
        }

        public void Connect()
        {
            if (client != null && (!client.IsConnected))
                client.Connect(Fhost, Fport);
        }

        public void Disconnect()
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }

        #endregion
    }
}