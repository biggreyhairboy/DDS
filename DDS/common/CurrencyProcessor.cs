using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common
{
    public class CurrencyRatioUpdateEventArgs : EventArgs
    {
        protected CurrencyData data;

        public CurrencyRatioUpdateEventArgs(CurrencyData data)
        {
            this.data = data;
        }

        public CurrencyData Data { get { return data; } }
    }

    public class CurrencyData
    {
        public event EventHandler<CurrencyRatioUpdateEventArgs> OnRatioUpdate;

        protected string name;
        protected decimal ratio = 1.0m;
        protected bool isValid;

        public CurrencyData(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }

        public decimal Ratio
        {
            get { return ratio; }
            internal set
            {
                if (ratio != value)
                {
                    ratio = value;
                    FireOnRatioUpdate();
                }
            }
        }

        public bool IsValid
        {
            get { return isValid; }
            internal set { isValid = value; }
        }

        private void FireOnRatioUpdate()
        {
            if (OnRatioUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnRatioUpdate(this, new CurrencyRatioUpdateEventArgs(this));
                else omsCommon.SyncInvoker.Invoke(OnRatioUpdate, new object[] { this, new CurrencyRatioUpdateEventArgs(this) });
            }
        }
    }

    public class CurrencyProcessor
    {
        public event EventHandler<CurrencyRatioUpdateEventArgs> OnRatioUpdate;

        private static CurrencyProcessor instance;
        private static volatile object syncRoot = new object();

        protected Dictionary<string, CurrencyData> cashSymbols;
        protected bool needSubscribeRatio;
        protected SubscribeManager submgr;

        public CurrencyProcessor(List<string> cashSymbols)
        {
            needSubscribeRatio = true;
            AddCashSymbols(cashSymbols);
        }

        public SubscribeManager Submgr
        {
            get
            {
                if (submgr == null)
                    submgr = SubscribeManager.Instance;
                return submgr;
            }
            set { submgr = value; }
        }

        public void AddCashSymbols(List<string> symbols)
        {
            if (symbols == null) return;
            if (symbols.Count == 0) return;
            foreach (string symbol in symbols)
            {
                AddCashSymbol(symbol);
            }
        }

        public void AddCashSymbol(string symbol)
        {
            if (cashSymbols == null) cashSymbols = new Dictionary<string, CurrencyData>();
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(cashSymbols);
            try
            {
                if (!cashSymbols.ContainsKey(symbol))
                {
                    CurrencyData item = new CurrencyData(symbol);
                    cashSymbols[symbol] = item;
                    if (needSubscribeRatio)
                        SubscribeRatio(symbol);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(cashSymbols);
            }
        }

        public bool IsCashSymbol(string symbol)
        {
            if (cashSymbols != null && cashSymbols.Count > 0 && symbol != null && symbol.Trim() != "")
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(cashSymbols);
                try
                {
                    return cashSymbols.ContainsKey(symbol);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(cashSymbols);
                }
            }
            return false;
        }

        public decimal InBaseCurrency(string currency, decimal price)
        {
            if ((currency == null || currency.Trim() == "") || (currency == omsCommon.BasicCurrency))
            {
                return price;
            }
            else
            {
                CurrencyData data = CurrencyOf(currency);
                if (data != null && data.Ratio > 0) return price * data.Ratio;
                else
                {
                    TLog.DefaultInstance.WriteLog(string.Format("FX Rate of {0} not found!", currency), LogType.ERROR);
                    return price;
                }
            }
        }
        /// <summary>
        /// Gets the currency data for a specified currency
        /// </summary>
        /// <param name="currency">Currency without "="</param>
        /// <returns>CurrencyData</returns>
        public CurrencyData CurrencyOf(string currency)
        {
            SubscribeManager.Instance.RequestVerify();
            if (currency == null || currency.Trim() == "") return null;
            if (cashSymbols == null) cashSymbols = new Dictionary<string, CurrencyData>();
            AddCashSymbol(currency);
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(cashSymbols);
            try
            {
                return cashSymbols[currency];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(cashSymbols);
            }
        }

        protected void SubscribeRatio(string currency)
        {
            if (currency == null || currency.Trim() == "") return;
            SubscribeResult item = Submgr.SubscribeBySymbol(currency + "=");
            if (item.IsValid)
            {
                ProcessRatioUpdate(this, new SubscribeResultEventArgs(item));
            }
            else item.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessRatioUpdate));
        }

        private void ProcessRatioUpdate(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                string tmpCurrency = e.Result.GetAttributeAsString(omsConst.OMS_SYMBOL);
                if (tmpCurrency.Length > 0)
                    tmpCurrency = tmpCurrency.Substring(0, tmpCurrency.Length - 1);
                if (tmpCurrency.Trim() == "") return;
                AddCashSymbol(tmpCurrency);
                CurrencyData data = cashSymbols[tmpCurrency];
                if (data == null) return;
                decimal ratio = e.Result.GetAttributeAsDecimal(omsConst.OMS_L_PRICE);
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(data);
                try
                {
                    data.IsValid = true;
                    if (ratio == 0) ratio = 1.0m;
                    if (data.Ratio != ratio)
                    {
                        data.Ratio = ratio;
                        FireOnRatioUpdate(data);
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(data);
                }
            }
        }

        private void FireOnRatioUpdate(CurrencyData data)
        {
            if (OnRatioUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnRatioUpdate(this, new CurrencyRatioUpdateEventArgs(data));
                else omsCommon.SyncInvoker.Invoke(OnRatioUpdate, new object[] { this, new CurrencyRatioUpdateEventArgs(data) });
            }
        }

        public bool NeedSubscribeRatio { get { return needSubscribeRatio; } set { needSubscribeRatio = value; } }

        public static CurrencyProcessor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            List<string> items = new List<string>();
                            items.Add(omsCommon.BasicCurrency);
                            instance = new CurrencyProcessor(items);
                        }
                    }
                }
                return instance;
            }
        }

        public static void SetInstance(CurrencyProcessor item)
        {
            instance = item;
        }
    }
}
