using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common
{
    public class MarketItem
    {
        public const int INVALIDMARKETSTATE = -9999;
        public const string MAINMARKET = "MAIN";

        protected string exchange;
        protected Dictionary<string, int> marketStatus;

        public MarketItem(string exch)
        {
            this.exchange = exch;
            marketStatus = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string Exchange { get { return exchange; } }

        public int MarketStatus { get { return MarketStatusOf(MAINMARKET); } }

        internal void UpdateMarketStatus(string mkt, int state)
        {
            if (mkt == null) return;
            //if (mkt.Trim() == "") return;
            omsCommon.AcquireSyncLock(marketStatus);
            try
            {
                marketStatus[mkt.Trim()] = state;
            }
            finally
            {
                omsCommon.ReleaseSyncLock(marketStatus);
            }
        }

        public int MarketStatusOf(string mkt)
        {
            if (mkt != null)
            {
                omsCommon.AcquireSyncLock(marketStatus);
                try
                {
                    if (marketStatus.ContainsKey(mkt))
                        return marketStatus[mkt];
                }
                finally
                {
                    omsCommon.ReleaseSyncLock(marketStatus);
                }
            }

            return INVALIDMARKETSTATE;
        }
    }

    public class MarketStatusChangeEventArgs : EventArgs
    {
        protected MarketItem item;

        public MarketStatusChangeEventArgs(MarketItem item)
        {
            this.item = item;
        }

        public MarketItem Market { get { return item; } }
    }

    public class OmsMarketManager
    {
        public event EventHandler<MarketStatusChangeEventArgs> OnMarketStatusChanged;

        private static OmsMarketManager instance;
        private static volatile object syncRoot = new object();

        protected Dictionary<string, MarketItem> innerMarkets;
        protected SubscribeManager submgr;
        protected bool subscribed;

        public OmsMarketManager()
        {
            innerMarkets = new Dictionary<string, MarketItem>();
            SubscribeMarketStatus();
        }

        public void SubscribeMarketStatus()
        {
            if (subscribed) return;
            subscribed = true;
            SubscribeResult res = Submgr.SubscribeBySymbol("EXCH_STATUS");
            res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessMarketStatus));
        }

        public bool IsMarketOpen(string exch)
        {
            return IsMarketOpen(exch, MarketItem.MAINMARKET);
        }

        public bool IsMarketOpen(string exch, string mkt)
        {
            if (exch == null || exch.Trim() == "") return false;
            omsCommon.AcquireSyncLock(innerMarkets);
            try
            {
                if (innerMarkets.ContainsKey(exch))
                {
                    MarketItem item = innerMarkets[exch];
                    int state = item.MarketStatusOf(mkt);
                    if (state != MarketItem.INVALIDMARKETSTATE)
                        if (state == omsConst.omsMarketOpen) return true;
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerMarkets);
            }
            return false;
        }

        public bool IsMarketClosed(string exch)
        {
            return IsMarketClosed(exch, MarketItem.MAINMARKET);
        }

        public bool IsMarketClosed(string exch, string mkt)
        {
            if (exch == null || exch.Trim() == "") return false;
            omsCommon.AcquireSyncLock(innerMarkets);
            try
            {
                if (innerMarkets.ContainsKey(exch))
                {
                    MarketItem item = innerMarkets[exch];
                    int state = item.MarketStatusOf(mkt);
                    if (state != MarketItem.INVALIDMARKETSTATE)
                        if (state == omsConst.omsMarketClosed) return true;
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerMarkets);
            }
            return false;
        }

        private void ProcessMarketStatus(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                TLog.DefaultInstance.WriteLog("DDS>|" + e.Result.ToString(), LogType.INFO);
                string exch = e.Result.GetAttributeAsString(omsConst.OMS_EXCHANGE);
                if (exch != null && exch.Trim() != "")
                {
                    if (e.Result.ContainsKey(omsConst.OMS_STATUS))
                    {
                        int status = e.Result.GetAttributeAsInteger(omsConst.OMS_STATUS);
                        string mkt = "";
                        if (e.Result.ContainsKey(omsConst.OMS_EXCH_MARKET))
                            mkt = e.Result.GetAttributeAsString(omsConst.OMS_EXCH_MARKET);
                        MarketItem item = null;
                        omsCommon.AcquireSyncLock(innerMarkets);
                        try
                        {
                            if (innerMarkets.ContainsKey(exch))
                            {
                                item = innerMarkets[exch];
                            }
                            else
                            {
                                item = new MarketItem(exch);
                                innerMarkets[exch] = item;
                            }
                            item.UpdateMarketStatus(mkt, status);
                        }
                        finally
                        {
                            omsCommon.ReleaseSyncLock(innerMarkets);
                        }
                        FireOnMarketStatusChanged(item);
                    }
                }
            }
        }

        private void FireOnMarketStatusChanged(MarketItem item)
        {
            if (OnMarketStatusChanged != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnMarketStatusChanged(this, new MarketStatusChangeEventArgs(item));
                else omsCommon.SyncInvoker.Invoke(OnMarketStatusChanged, new object[] { this, new MarketStatusChangeEventArgs(item) });
            }
        }

        public SubscribeManager Submgr
        {
            get
            {
                if (submgr == null) submgr = SubscribeManager.Instance;
                return submgr;
            }
            set { submgr = value; }
        }

        public static OmsMarketManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new OmsMarketManager();
                    }
                }
                return instance;
            }
        }
    }
}
