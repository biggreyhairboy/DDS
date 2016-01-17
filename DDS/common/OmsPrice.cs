using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common
{
    public class PriceUpdateEventArgs : EventArgs
    {
        protected OmsPrice price;

        public PriceUpdateEventArgs(OmsPrice price)
        {
            this.price = price;
        }

        public OmsPrice Price { get { return price; } }
    }

    public class OmsPrice : IDisposable
    {
        private static volatile object priceSyncRoot = new object();
        internal static Dictionary<string, OmsPrice> prices;
        public event EventHandler<PriceUpdateEventArgs> OnPriceUpdate;

        protected string symbol;
        protected decimal bid;
        protected decimal ask;
        protected decimal last;
        protected decimal close;
        protected string exch;
        protected string status;
        protected int prodType;
        protected string currency;
        protected bool dirtyBit;
        private volatile object syncRoot = new object();
        protected CurrencyData currencyRate;
        protected SubscribeResult subItem;
        protected SubscribeManager submgr;

        public OmsPrice(string symbol)
        {
            this.symbol = symbol;
            SubscribePrice();
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

        private void SubscribePrice()
        {
            subItem = Submgr.SubscribePrice(symbol);
            subItem.AddHandler(new EventHandler<SubscribeResultEventArgs>(PriceUpdate));
        }

        private void PriceUpdate(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                int tmpProdType = e.Result.GetAttributeAsInteger(omsConst.OMS_PRODTYPE);
                decimal tmpClose = e.Result.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
                decimal tmpLast = e.Result.GetAttributeAsDecimal(omsConst.OMS_L_PRICE);
                decimal tmpBid = e.Result.GetAttributeAsDecimal(omsConst.OMS_BID);
                decimal tmpAsk = e.Result.GetAttributeAsDecimal(omsConst.OMS_OFFER);
                string tmpExchange = e.Result.GetAttributeAsString(omsConst.OMS_EXCHANGE);
                string tmpCurrency = e.Result.GetAttributeAsString(omsConst.OMS_CURRENCY);
                if (tmpCurrency.Length > 3) tmpCurrency = tmpCurrency.Substring(0, 3);
                string tmpStatus = e.Result.GetAttributeAsString(omsConst.OMS_STATUS);
                if ((tmpProdType != prodType) || (tmpClose != close) || (tmpLast != last) || (tmpCurrency != currency) || (tmpStatus != status)) dirtyBit = true;

                omsCommon.AcquireSyncLock(syncRoot);
                try
                {
                    prodType = tmpProdType;
                    close = tmpClose;
                    last = tmpLast;
                    bid = tmpBid;
                    ask = tmpAsk;
                    exch = tmpExchange;
                    currency = tmpCurrency;
                    status = tmpStatus;

                    if (currency != null && currency.Trim() != "")
                    {
                        currencyRate = CurrencyProcessor.Instance.CurrencyOf(currency);
                        currencyRate.OnRatioUpdate += new EventHandler<CurrencyRatioUpdateEventArgs>(currencyRate_OnRatioUpdate);
                    }
                }
                finally
                {
                    omsCommon.ReleaseSyncLock(syncRoot);
                }
                FireOnPriceUpdate();
            }
        }

        protected void currencyRate_OnRatioUpdate(object sender, CurrencyRatioUpdateEventArgs e)
        {
            FireOnPriceUpdate();
        }

        private void FireOnPriceUpdate()
        {
            if (OnPriceUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnPriceUpdate(this, new PriceUpdateEventArgs(this));
                else omsCommon.SyncInvoker.Invoke(OnPriceUpdate, new object[] { this, new PriceUpdateEventArgs(this) });
            }
        }

        public bool IsReady
        {
            get
            {
                bool currencyReady = true;
                bool ready = false;
                if (subItem != null) ready = subItem.IsValid;
                if (ready && (!symbol.EndsWith("=")) && (currency != null && currency.Trim() != "") && (currency != omsCommon.BasicCurrency))
                {
                    if (currencyRate == null)
                    {
                        currencyRate = CurrencyProcessor.Instance.CurrencyOf(currency);
                        currencyRate.OnRatioUpdate += new EventHandler<CurrencyRatioUpdateEventArgs>(currencyRate_OnRatioUpdate);
                    }
                    currencyReady = currencyRate.IsValid;
                }

                return ready && currencyReady;
            }
        }

        public bool IsValid
        {
            get
            {
                bool res = false;
                if (subItem != null) res = subItem.IsValid;
                if (res && (!symbol.EndsWith("=")) && (currency != null && currency.Trim() != "") && (currency != omsCommon.BasicCurrency))
                {
                    if (currencyRate == null)
                    {
                        currencyRate = CurrencyProcessor.Instance.CurrencyOf(currency);
                        currencyRate.OnRatioUpdate += new EventHandler<CurrencyRatioUpdateEventArgs>(currencyRate_OnRatioUpdate);
                    }
                    res = currencyRate.IsValid;
                }
                return res;
            }
        }

        public static OmsPrice PriceOf(string symbol)
        {
            SubscribeManager.Instance.RequestVerify();
            if (symbol == null || symbol.Trim() == "") return null;
            omsCommon.AcquireSyncLock(priceSyncRoot);
            try
            {
                if (prices == null) prices = new Dictionary<string, OmsPrice>();
            }
            finally
            {
                omsCommon.ReleaseSyncLock(priceSyncRoot);
            }

            omsCommon.AcquireSyncLock(prices);
            try
            {
                if (prices.ContainsKey(symbol))
                {
                    return prices[symbol];
                }
                else
                {
                    OmsPrice item = new OmsPrice(symbol);
                    prices[symbol] = item;
                    return item;
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(prices);
            }
        }

        public SubscribeResult SubItem { get { return subItem; } }

        public string Symbol { get { return symbol; } }

        public decimal Bid { get { return bid; } set { bid = value; } }

        public decimal Ask { get { return ask; } set { ask = value; } }

        public decimal Last { get { return last; } set { last = value; } }

        public decimal Close { get { return close; } set { close = value; } }

        public string Exchange { get { return exch; } set { exch = value; } }

        public int ProductType { get { return prodType; } set { prodType = value; } }

        public string Currency { get { return currency; } set { currency = value; } }

        public decimal Nominal
        {
            get
            {
                decimal res = 0m;
                if (subItem != null && subItem.IsValid)
                {
                    res = Last;
                    if (res == 0)
                    {
                        decimal b = Bid;
                        decimal p = Close;
                        decimal a = Ask;
                        if ((a == 0) && (b == 0) && (p == 0)) res = 0;
                        else if (p > 0) res = p;
                        else if ((b > 0) && (a > 0))
                            res = (a + b) / 2;
                        else res = 0;
                    }
                }

                return res;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            OnPriceUpdate = null;
            if (subItem != null)
                subItem.ResetHandler();
        }

        #endregion
    }
}