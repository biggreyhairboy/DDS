using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.Threading;

namespace OMS.common
{
    public class OmsSessionManager
    {
        public event EventHandler<OmsSessionStateEventArgs> OnSessionStateChanged;

        protected string sessionCode;
        protected string userID;
        protected string applicationID;
        protected string sessionID;
        protected string heartbeatRefFlag;
        protected int heartbeatStatus;
        protected int ssmHeartbeat;
        protected int heartbeatInterval;
        protected Timer heartbeatTimer;

        public OmsSessionManager(string userID, string appID, string sessionID, int ssmHeartbeat)
        {
            heartbeatStatus = 0;
            this.userID = userID;
            this.applicationID = appID;
            this.sessionID = sessionID;
            this.ssmHeartbeat = ssmHeartbeat;
            heartbeatTimer = new Timer(new TimerCallback(HeartbeatTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool IsValid
        {
            get
            {
                if (userID == null || userID.Trim() == "") return false;
                if (applicationID == null || applicationID.Trim() == "") return false;
                if (sessionID == null || sessionID.Trim() == "") return false;
                return true;
            }
        }

        public string UserID { get { return userID; } }

        public string ApplicationID { get { return applicationID; } }

        public string SessionID { get { return sessionID; } }

        public int SSMHeartbeat { get { return ssmHeartbeat; } }

        public string HeartbeatRefFlag { get { return heartbeatRefFlag; } set { heartbeatRefFlag = value; } }

        public int HeartbeatStatus { get { return heartbeatStatus; } set { heartbeatStatus = value; } }
        /// <summary>
        /// Gets or sets the session symbol code for detecting session status
        /// </summary>
        public string SessionCode
        {
            get { return sessionCode; }
            set
            {
                if (sessionCode != value)
                {
                    try
                    {
                        if (sessionCode != null && sessionCode.Trim() != "")
                        {
                            if (SubscribeManager.Instance != null)
                            {
                                SubscribeResult subitem = SubscribeManager.Instance.SubscribeBySymbol(sessionCode);
                                if (subitem != null)
                                    subitem.RemoveHandler(ProcessSessionUpdate);

                                SubscribeResult testSubitem = SubscribeManager.Instance.SubscribeBySymbol("testlicense_ssm");
                                if (testSubitem != null)
                                    testSubitem.RemoveHandler(ProcessTestLicense);
                            }
                        }
                        if (value == null) return;
                        if (value.Trim() == "") return;
                        sessionCode = value.Trim();
                        if (SubscribeManager.Instance != null)
                        {
                            SubscribeResult subitem = SubscribeManager.Instance.SubscribeBySymbol(sessionCode);
                            subitem.AddHandler(ProcessSessionUpdate);

                            SubscribeResult testSubitem = SubscribeManager.Instance.SubscribeBySymbol("testlicense_ssm");
                            testSubitem.AddHandler(ProcessTestLicense);
                        }
                    }
                    catch (Exception ex)
                    {
                        TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the session heartbeat interval, in second
        /// </summary>
        public int HeartbeatInterval
        {
            get { return heartbeatInterval; }
            set
            {
                if (value < 1) return;
                if (heartbeatInterval != value)
                {
                    heartbeatInterval = value;
                    heartbeatTimer.Change(heartbeatInterval, heartbeatInterval);
                }
            }
        }

        protected virtual void ProcessTestLicense(object sender, SubscribeResultEventArgs e)
        {
            try
            {
                string appID = e.Result.GetAttributeAsString(99);
                string aUser = e.Result.GetAttributeAsString(13);
                if (appID == applicationID && aUser == userID)
                {
                    HeartbeatTimerCallback(null);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        protected virtual void ProcessSessionUpdate(object sender, SubscribeResultEventArgs e)
        {
            try
            {
                string appID = e.Result.GetAttributeAsString(99);
                string aUser = e.Result.GetAttributeAsString(13);
                string aSession = e.Result.GetAttributeAsString(1);
                if (appID == applicationID && aUser == userID && aSession == sessionID)
                {
                    int stateCode = e.Result.GetAttributeAsInteger(5);
                    SSMSessionState state = (SSMSessionState)stateCode;
                    if (OnSessionStateChanged != null)
                        OnSessionStateChanged(this, new OmsSessionStateEventArgs(state));
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        protected virtual void HeartbeatTimerCallback(object target)
        {
            if (!IsValid) return;
            if (SubscribeManager.Instance == null) return;
            StringBuilder buffer = new StringBuilder("image|heartbeat_ssm|0|heartbeat_ssm|");
            buffer.Append(string.Format("13|{0}|", userID));
            buffer.Append(string.Format("99|{0}|", applicationID));
            buffer.Append(string.Format("4|{0}|", ssmHeartbeat));
            buffer.Append(string.Format("5|{0}|", heartbeatStatus));
            buffer.Append(string.Format("1|{0}|", sessionID));
            if (heartbeatRefFlag != null && heartbeatRefFlag.Trim() != "")
                buffer.Append(string.Format("74|{0}|", heartbeatRefFlag));
            SubscribeManager.Instance.SendDDSMessage(buffer.ToString());
        }
        /// <summary>
        /// Notify SSM session has been logout
        /// </summary>
        public virtual void Logout()
        {
            try
            {
                if (!IsValid) return;
                if (SubscribeManager.Instance == null) return;
                StringBuilder buffer = new StringBuilder("image|logout_ssm|0|logout_ssm|");
                buffer.Append(string.Format("13|{0}|", userID));
                buffer.Append(string.Format("99|{0}|", applicationID));
                buffer.Append(string.Format("1|{0}|", sessionID));
                SubscribeManager.Instance.SendDDSMessage(buffer.ToString());
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }
    }

    public class OmsSessionStateEventArgs : EventArgs
    {
        protected SSMSessionState sessionState;

        public OmsSessionStateEventArgs(SSMSessionState sessionState)
        {
            this.sessionState = sessionState;
        }

        public SSMSessionState SessionState { get { return sessionState; } }
    }
}
