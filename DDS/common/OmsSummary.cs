using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OMS.common.Utilities;
using System.Reflection;

namespace OMS.common
{
    public class OmsSummary : IDisposable
    {
        public event EventHandler<SummaryUpdateEventArgs> OnSummaryUpdate;

        protected object syncRoot = new object();
        protected Timer summaryTimer;
        protected string name;
        protected int summaryInterval;
        protected int currentTag;
        protected int startTag;
        protected string appVersion;
        protected string appStartupTime;

        public OmsSummary(string name)
        {
            this.name = name;
            if (name == null || name.Trim() == "")
            {
                TLog.DefaultInstance.WriteLog("OmsSummary detect invalid name!!", LogType.ERROR);
            }
            currentTag = 0;
            startTag = 200;
            summaryTimer = new Timer(new TimerCallback(SummaryTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
            appStartupTime = DateTime.Now.ToString("HH:mm:ss");
            try
            {
                Assembly asm = Assembly.GetCallingAssembly();
                if (asm == null) asm = Assembly.GetEntryAssembly();
                if (asm == null) asm = Assembly.GetExecutingAssembly();
                if (asm != null)
                {
                    appVersion = asm.GetName().Version.ToString();
                }
            }
            catch { }
        }

        public string Name { get { return name; } }
        /// <summary>
        /// Gets or sets the summary interval, in second
        /// </summary>
        public int SummaryInterval
        {
            get { return summaryInterval; }
            set
            {
                if (summaryInterval != value)
                {
                    summaryInterval = value;
                    summaryTimer.Change(summaryInterval * 1000, summaryInterval * 1000);
                }
            }
        }

        public int CurrentTag { get { return currentTag; } }

        public string AppVersion { get { return appVersion; } set { appVersion = value; } }

        public string AppStartupTime { get { return appStartupTime; } set { appStartupTime = value; } }

        protected virtual string InitialSummary()
        {
            StringBuilder buffer = new StringBuilder(string.Format("image|SUMMARY_{0}|", name));
            currentTag = startTag;
            buffer.Append(string.Format("{0}|110:Version::S|", currentTag));
            currentTag += 1;
            buffer.Append(string.Format("{0}|501:StartupTime::S|", currentTag));
            currentTag += 1;
            buffer.Append(string.Format("{0}|33:Time::S|", currentTag));
            currentTag += 1;
            buffer.Append(string.Format("{0}|4:Heartbeat::S|", currentTag));
            return buffer.ToString();
        }

        protected virtual void SummaryTimerCallback(object target)
        {
            if (name == null || name.Trim() == "") return;
            string msg = "";
            lock (syncRoot)
            {
                StringBuilder buffer = new StringBuilder(InitialSummary());
                buffer.Append(string.Format("110|{0}|", appVersion));
                buffer.Append(string.Format("501|{0}|", appStartupTime));
                buffer.Append(string.Format("33|{0}|", DateTime.Now.ToString("HH:mm:ss")));
                buffer.Append(string.Format("4|{0}|", summaryInterval * 2));
                buffer.Append("502|SUMMARY|");
                msg = buffer.ToString();
            }
            if (msg != null && msg.Trim() != "")
            {
                if (OnSummaryUpdate != null)
                    OnSummaryUpdate(this, new SummaryUpdateEventArgs(msg));
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (summaryTimer != null)
                {
                    summaryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    summaryTimer.Dispose();
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion
    }

    public class SummaryUpdateEventArgs : EventArgs
    {
        protected string msg;

        public SummaryUpdateEventArgs(string msg)
        {
            this.msg = msg;
        }

        public string Message { get { return msg; } }
    }
}
