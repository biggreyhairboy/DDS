using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using OMS.common.Models.DataModel;

namespace OMS.common.Models.PositionModel
{
    public class PositionData : MarginRatioSubscriber
    {
        public event EventHandler<EventArgs> OnMarginRatioUpdate;
        protected decimal quantity;
        protected decimal shortQuantity;
        protected decimal shortSellQuantity;
        protected decimal avgPrice;
        protected decimal param1Quantity;
        protected decimal param2Quantity;
        protected decimal param3Quantity;
        protected decimal param4Quantity;
        protected decimal todayBuyQuantity;
        protected decimal todaySellQuantity;
        protected decimal pendingBuyQuantity;
        protected decimal pendingSellQuantity;
        protected decimal todayIBuyQuantity;
        protected decimal todayISellQuantity;
        protected decimal pendingIBuyQuantity;
        protected decimal pendingISellQuantity;
        protected decimal depositQuantity;
        protected decimal withdrawQuantity;
        protected decimal todayBuyValue;
        protected decimal todaySellValue;
        protected decimal todayShortSellValue;
        protected decimal pendingBuyValue;
        protected decimal pendingSellValue;
        protected decimal todayIBuyValue;
        protected decimal todayISellValue;
        protected decimal pendingIBuyValue;
        protected decimal pendingISellValue;
        protected decimal bodTradableQuantity;
        protected decimal bodWithdrawableQuantity;
        protected decimal converedQuantity;
        protected decimal tradableQuantity;
        protected decimal withdrawableQuantity;
        protected decimal unclearChequeQuantity;
        protected decimal cashDepositAmount;
        protected decimal cashWithdrawAmount;
        protected decimal gTCCashDepositAmount;
        protected decimal gTCCashWithdrawAmount;
        protected decimal gTCTLDepositAmount;
        protected decimal gTCTLWithdrawAmount;
        protected decimal gTCTradeQuantityWithdraw;
        protected decimal gTCTradeQuautityDeposit;
        protected decimal gTCWithdrawQuantityWithdraw;
        protected decimal gTCWithdrawQuantityDeposit;
        protected decimal tradeLimitDepositAmount;
        protected decimal tradeLimitWithdrawAmount;
        protected OmsPrice ticker;
        protected string symbol;
        protected decimal preClose;
        protected bool isCurrency;
        protected bool dirty;
        protected decimal loanAmount;
        protected decimal acceptValue;
        protected decimal bodQuantity;
        protected decimal bodShortQuantity;
        protected decimal bodAvgPrice;
        protected decimal marginRatio = 1m;
        protected List<string> siOPFlags;
        protected ProfitLossBase pnl;
        protected DayTradeProfitLoss dayTradePL;
        protected decimal todayOpenLong;
        protected decimal todayCloseLong;
        protected decimal pendingOpenLong;
        protected decimal pendingCloseLong;
        protected decimal todayOpenShort;
        protected decimal todayCloseShort;
        protected decimal pendingOpenShort;
        protected decimal pendingCloseShort;

        public PositionData()
        {
            dirty = false;
            siOPFlags = new List<string>();
            pnl = new ProfitLossBase(this);
            dayTradePL = new DayTradeProfitLoss(this);
        }

        public void Assign(PositionData item)
        {
            if (item == null) return;
            this.acceptValue = item.AcceptValue;
            this.avgPrice = item.AvgPrice;
            this.bodTradableQuantity = item.BodTradableQuantity;
            this.bodWithdrawableQuantity = item.BodWithdrawableQuantity;
            this.cashDepositAmount = item.CashDepositAmount;
            this.cashWithdrawAmount = item.CashWithdrawAmount;
            this.converedQuantity = item.ConveredQuantity;
            this.depositQuantity = item.DepositQuantity;
            this.gTCCashDepositAmount = item.GTCCashDepositAmount;
            this.gTCCashWithdrawAmount = item.GTCCashWithdrawAmount;
            this.gTCTLDepositAmount = item.GTCTLDepositAmount;
            this.gTCTLWithdrawAmount = item.GTCTLWithdrawAmount;
            this.gTCTradeQuantityWithdraw = item.GTCTradeQuantityWithdraw;
            this.gTCTradeQuautityDeposit = item.GTCTradeQuautityDeposit;
            this.gTCWithdrawQuantityDeposit = item.GTCWithdrawQuantityDeposit;
            this.gTCWithdrawQuantityWithdraw = item.GTCWithdrawQuantityWithdraw;
            this.isCurrency = item.IsCurrency;
            this.loanAmount = item.LoanAmount;
            this.param1Quantity = item.Param1Quantity;
            this.param2Quantity = item.Param2Quantity;
            this.param3Quantity = item.Param3Quantity;
            this.param4Quantity = item.Param4Quantity;
            this.pendingBuyQuantity = item.PendingBuyQuantity;
            this.pendingBuyValue = item.PendingBuyValue;
            this.pendingIBuyQuantity = item.PendingIBuyQuantity;
            this.pendingIBuyValue = item.PendingIBuyValue;
            this.pendingISellQuantity = item.PendingISellQuantity;
            this.pendingISellValue = item.PendingISellValue;
            this.pendingSellQuantity = item.PendingSellQuantity;
            this.pendingSellValue = item.PendingSellValue;
            this.pnl = item.PL;
            this.dayTradePL = item.DayTradePL;
            this.preClose = item.PreClose;
            this.quantity = item.Quantity;
            this.shortQuantity = item.ShortQuantity;
            this.symbol = item.Symbol;
            this.ticker = item.Ticker;
            this.todayBuyQuantity = item.TodayBuyQuantity;
            this.todayBuyValue = item.TodayBuyValue;
            this.todayIBuyQuantity = item.TodayIBuyQuantity;
            this.todayIBuyValue = item.TodayIBuyValue;
            this.todayISellQuantity = item.TodayISellQuantity;
            this.todayISellValue = item.TodayISellValue;
            this.todaySellQuantity = item.TodaySellQuantity;
            this.todaySellValue = item.TodaySellValue;
            this.tradableQuantity = item.TradableQuantity;
            this.tradeLimitDepositAmount = item.TradeLimitDepositAmount;
            this.tradeLimitWithdrawAmount = item.TradeLimitWithdrawAmount;
            this.unclearChequeQuantity = item.UnclearChequeQuantity;
            this.withdrawableQuantity = item.WithdrawableQuantity;
            this.withdrawQuantity = item.WithdrawQuantity;
            this.bodAvgPrice = item.BodAvgPrice;
            this.bodQuantity = item.BodQuantity;
            this.bodShortQuantity = item.BodShortQuantity;
            this.siOPFlags = item.SIOperatorFlag;
            this.marginRatio = item.marginRatio;
            this.todayOpenLong = item.TodayOpenLong;
            this.todayCloseLong = item.TodayCloseLong;
            this.pendingOpenLong = item.PendingOpenLong;
            this.pendingCloseLong = item.PendingCloseLong;
            this.todayOpenShort = item.TodayOpenShort;
            this.todayCloseShort = item.TodayCloseShort;
            this.pendingOpenShort = item.PendingOpenShort;
            this.PendingCloseShort = item.PendingCloseShort;
            this.shortSellQuantity = item.ShortSellQuantity;
            this.todayShortSellValue = item.TodayShortSellValue;
        }

        internal void InitBODQuantity(decimal bodQty)
        {
            pnl.InitBodQuantity(bodQty);
            dayTradePL.InitBodQuantity(bodQty);
        }

        internal void CalcPL(amsTrade trade)
        {
            if (trade == null) return;
            pnl.AddTrade(trade);
            dayTradePL.AddTrade(trade);
        }

        internal void RecalcPL()
        {
            pnl.Recalc();
            dayTradePL.Recalc();
        }

        public bool Dirty
        {
            get { return dirty; }
            internal set
            {
                if (dirty != value) dirty = value;
            }
        }

        public List<string> SIOperatorFlag { get { return siOPFlags; } set { siOPFlags = value; } }

        public ProfitLossBase PL { get { return pnl; } }

        public DayTradeProfitLoss DayTradePL {get { return dayTradePL; } }

        public decimal AcceptValue
        {
            get { return acceptValue; }
            set { acceptValue = value; }
        }

        public decimal LoanAmount
        {
            get { return loanAmount; }
            set { loanAmount = value; }
        }

        public OmsPrice Ticker { get { return ticker; } }

        public string ChiSymbolName
        {
            get
            {
                string res = "";
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsString(omsConst.OMS_CHI_NAME);
                }
                return res;
            }
        }

        public string EngSymbolName
        {
            get
            {
                string res = "";
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsString(omsConst.OMS_NAME);
                }
                return res;
            }
        }

        public string ShortSymbolName
        {
            get
            {
                string res = "";
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    string shortName = ticker.SubItem.GetAttributeAsString(omsConst.OMS_ACCOUNT);
                    if (shortName != null && shortName.Trim() != "") res = shortName;
                    else
                    {
                        shortName = ticker.SubItem.GetAttributeAsString(omsConst.OMS_NAME);
                        if (shortName != null && shortName.Trim() != "") res = shortName;
                    }
                }
                return res;
            }
        }

        public decimal Price
        {
            get
            {
                if (avgPrice > 0) return avgPrice;
                else return PreClose;
            }
        }

        public int ProductType
        {
            get
            {
                int res = 0;
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsInteger(omsConst.OMS_PRODTYPE);
                }
                return res;
            }
        }

        public decimal Bid
        {
            get
            {
                decimal res = 0m;
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_BID);
                }
                return res;
            }
        }

        public decimal Nominal
        {
            get
            {
                decimal res = 0m;
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_L_PRICE);
                    if (res == 0)
                    {
                        decimal b = Bid;
                        decimal p = PreClose;
                        decimal a = ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_OFFER);
                        if ((a == 0) && (b == 0) && (p == 0)) res = 0;
                        else if (p > 0) res = p;
                        else if ((b > 0) && (a > 0))
                            res = (a + b) / 2;
                        else res = 0;
                    }
                }
                if (omsCommon.LogDebugInfo && res < 0 && null!=ticker)
                    TLog.DefaultInstance.WriteLog(string.Format("DEBUG NominalNegative Symbol:{0},LPrice:{1},Bid:{2},Ask:{3},OfferPrice:{4}", this.symbol, ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_L_PRICE), Bid, PreClose,ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_OFFER)), LogType.INFO);
                return res;
            }
        }

        public decimal MarketValue
        {
            get
            {
                if (null != ticker)
                {
                    string sysStatus = ticker.SubItem.GetAttributeAsString(omsConst.OMS_STATUS);
                    if ((ProductType != OmsOrdConst.omsProductFuture) && (ProductType != OmsOrdConst.omsProductOption) &&
                        (sysStatus != "10") && (sysStatus != "12"))
                        return quantity * Nominal;
                    else return 0m;
                }
                else return 0m;
            }
        }

        public decimal MarginRatio
        {
            get
            {
                return marginRatio;
            }
            set
            {
                marginRatio = value;
            }
        }

        public decimal MarginableValue
        {
            get
            {
                return MarketValue * MarginRatio;
            }
        }

        public decimal PreClose
        {
            get
            {
                if (preClose == 0)
                {
                    if (null != ticker && ticker.SubItem.IsValid)
                    {
                        preClose = ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
                    }
                }
                return preClose;
            }
            set { preClose = value; }
        }

        public decimal ContractSize
        {
            get
            {
                decimal res = 0m;
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_CONTRACT_SIZE);
                }
                if (res == 0) res = 1m;
                return res;
            }
        }

        public string Exchange
        {
            get
            {
                string res = "";
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsString(omsConst.OMS_EXCHANGE);
                }
                return res;
            }
        }

        public string Currency
        {
            get
            {
                string res = "";
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    res = ticker.SubItem.GetAttributeAsString(omsConst.OMS_CURRENCY);
                }
                return res;
            }
        }

        public OmsCallPut CallPut
        {
            get
            {
                if (null != ticker && ticker.SubItem.IsValid)
                {
                    string res = ticker.SubItem.GetAttributeAsString(omsConst.OMS_PUTCALL);
                    if (res != null && res.Trim() != "")
                    {
                        if ((res == "1") || (res == "6") || (res == "C")) return OmsCallPut.cpCall;
                        if ((res == "2") || (res == "7") || (res == "P")) return OmsCallPut.cpPut;
                    }
                }
                return OmsCallPut.cpUndefined;
            }
        }

        public string Symbol
        {
            get { return symbol; }
            set
            {
                if (symbol != value)
                {
                    symbol = value;
                    ticker = OmsPrice.PriceOf(symbol);
                    if (CurrencyProcessor.Instance != null)
                        isCurrency = CurrencyProcessor.Instance.IsCashSymbol(symbol);
                    if (omsCommon.NeedMarginRatio)
                        MarginRatio = MarginRatioManager.GetInstance("OMSDATA").GetMarginRatioOf(symbol, this);
                }
            }
        }

        public bool IsCurrency { get { return isCurrency; } }

        public decimal BodQuantity { get { return bodQuantity; } set { bodQuantity = value; } }

        public decimal BodShortQuantity { get { return bodShortQuantity; } set { bodShortQuantity = value; } }

        public decimal BodAvgPrice { get { return bodAvgPrice; } set { bodAvgPrice = value; } }

        public decimal TradeLimitWithdrawAmount
        {
            get { return tradeLimitWithdrawAmount; }
            set { tradeLimitWithdrawAmount = value; }
        }

        public decimal TradeLimitDepositAmount
        {
            get { return tradeLimitDepositAmount; }
            set { tradeLimitDepositAmount = value; }
        }

        public decimal GTCWithdrawQuantityDeposit
        {
            get { return gTCWithdrawQuantityDeposit; }
            set { gTCWithdrawQuantityDeposit = value; }
        }

        public decimal GTCWithdrawQuantityWithdraw
        {
            get { return gTCWithdrawQuantityWithdraw; }
            set { gTCWithdrawQuantityWithdraw = value; }
        }

        public decimal GTCTradeQuautityDeposit
        {
            get { return gTCTradeQuautityDeposit; }
            set { gTCTradeQuautityDeposit = value; }
        }

        public decimal GTCTradeQuantityWithdraw
        {
            get { return gTCTradeQuantityWithdraw; }
            set { gTCTradeQuantityWithdraw = value; }
        }

        public decimal GTCTLWithdrawAmount
        {
            get { return gTCTLWithdrawAmount; }
            set { gTCTLWithdrawAmount = value; }
        }

        public decimal GTCTLDepositAmount
        {
            get { return gTCTLDepositAmount; }
            set { gTCTLDepositAmount = value; }
        }

        public decimal GTCCashWithdrawAmount
        {
            get { return gTCCashWithdrawAmount; }
            set { gTCCashWithdrawAmount = value; }
        }

        public decimal GTCCashDepositAmount
        {
            get { return gTCCashDepositAmount; }
            set { gTCCashDepositAmount = value; }
        }

        public decimal CashWithdrawAmount
        {
            get { return cashWithdrawAmount; }
            set { cashWithdrawAmount = value; }
        }

        public decimal CashDepositAmount
        {
            get { return cashDepositAmount; }
            set { cashDepositAmount = value; }
        }

        public decimal UnclearChequeQuantity
        {
            get { return unclearChequeQuantity; }
            set { unclearChequeQuantity = value; }
        }

        public decimal WithdrawableQuantity
        {
            get { return withdrawableQuantity; }
            set { withdrawableQuantity = value; }
        }

        public decimal TradableQuantity
        {
            get { return tradableQuantity; }
            set { tradableQuantity = value; }
        }

        public decimal ConveredQuantity
        {
            get { return converedQuantity; }
            set { converedQuantity = value; }
        }

        public decimal BodWithdrawableQuantity
        {
            get { return bodWithdrawableQuantity; }
            set { bodWithdrawableQuantity = value; }
        }

        public decimal BodTradableQuantity
        {
            get { return bodTradableQuantity; }
            set { bodTradableQuantity = value; }
        }

        public decimal PendingISellValue
        {
            get { return pendingISellValue; }
            set { pendingISellValue = value; }
        }

        public decimal PendingIBuyValue
        {
            get { return pendingIBuyValue; }
            set { pendingIBuyValue = value; }
        }

        public decimal TodayISellValue
        {
            get { return todayISellValue; }
            set { todayISellValue = value; }
        }

        public decimal TodayIBuyValue
        {
            get { return todayIBuyValue; }
            set { todayIBuyValue = value; }
        }

        public decimal PendingSellValue
        {
            get { return pendingSellValue; }
            set { pendingSellValue = value; }
        }

        public decimal PendingBuyValue
        {
            get { return pendingBuyValue; }
            set { pendingBuyValue = value; }
        }

        public decimal TodaySellValue
        {
            get { return todaySellValue; }
            set { todaySellValue = value; }
        }

        public decimal TodayBuyValue
        {
            get { return todayBuyValue; }
            set { todayBuyValue = value; }
        }

        public decimal WithdrawQuantity
        {
            get { return withdrawQuantity; }
            set { withdrawQuantity = value; }
        }

        public decimal DepositQuantity
        {
            get { return depositQuantity; }
            set { depositQuantity = value; }
        }

        public decimal PendingISellQuantity
        {
            get { if (isCurrency) return 0m; return pendingISellQuantity; }
            set { pendingISellQuantity = value; }
        }

        public decimal PendingIBuyQuantity
        {
            get { if (isCurrency) return 0m; return pendingIBuyQuantity; }
            set { pendingIBuyQuantity = value; }
        }

        public decimal TodayISellQuantity
        {
            get { if (isCurrency) return 0m; return todayISellQuantity; }
            set { todayISellQuantity = value; }
        }

        public decimal TodayIBuyQuantity
        {
            get { if (isCurrency) return 0m; return todayIBuyQuantity; }
            set { todayIBuyQuantity = value; }
        }

        public decimal PendingSellQuantity
        {
            get { if (isCurrency) return 0m; return pendingSellQuantity; }
            set { pendingSellQuantity = value; }
        }

        public decimal PendingBuyQuantity
        {
            get { if (isCurrency) return 0m; return pendingBuyQuantity; }
            set { pendingBuyQuantity = value; }
        }

        public decimal TodaySellQuantity
        {
            get { if (isCurrency) return 0m; return todaySellQuantity; }
            set { todaySellQuantity = value; }
        }

        public decimal TodayBuyQuantity
        {
            get { if (isCurrency) return 0m; return todayBuyQuantity; }
            set { todayBuyQuantity = value; }
        }

        public decimal Param4Quantity
        {
            get { return param4Quantity; }
            set { param4Quantity = value; }
        }

        public decimal Param3Quantity
        {
            get { return param3Quantity; }
            set { param3Quantity = value; }
        }

        public decimal Param2Quantity
        {
            get { return param2Quantity; }
            set { param2Quantity = value; }
        }

        public decimal Param1Quantity
        {
            get { return param1Quantity; }
            set { param1Quantity = value; }
        }

        public decimal AvgPrice
        {
            get { return avgPrice; }
            set { avgPrice = value; }
        }

        public decimal ShortQuantity
        {
            get { return shortQuantity; }
            set { shortQuantity = value; }
        }

        public decimal ShortSellQuantity
        {
            get { return shortSellQuantity; }
            set { shortSellQuantity = value; }
        }

        public decimal Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                dirty = true;
            }
        }

        public decimal TodayOpenLong
        {
            get { return todayOpenLong; }
            set { todayOpenLong = value; }
        }

        public decimal TodayCloseLong
        {
            get { return todayCloseLong; }
            set { todayCloseLong = value; }
        }

        public decimal PendingOpenLong
        {
            get { return pendingOpenLong; }
            set { pendingOpenLong = value; }
        }

        public decimal PendingCloseLong
        {
            get { return pendingCloseLong; }
            set { pendingCloseLong = value; }
        }

        public decimal TodayOpenShort
        {
            get { return todayOpenShort; }
            set { todayOpenShort = value; }
        }

        public decimal TodayCloseShort
        {
            get { return todayCloseShort; }
            set { todayCloseShort = value; }
        }

        public decimal PendingOpenShort
        {
            get { return pendingOpenShort; }
            set { pendingOpenShort = value; }
        }

        public decimal PendingCloseShort
        {
            get { return pendingCloseShort; }
            set { pendingCloseShort = value; }
        }

        public decimal TodayShortSellValue
        {
            get { return todayShortSellValue; }
            set { todayShortSellValue = value; }
        }

        #region MarginRatioSubscriber Members

        string MarginRatioSubscriber.Symbol
        {
            get { return this.Symbol; }
        }

        void MarginRatioSubscriber.HandlerMarginRatioUpdate(decimal marginRatio)
        {
            MarginRatio = marginRatio;
            if (OnMarginRatioUpdate != null)
                OnMarginRatioUpdate(this,new EventArgs());
        }

        #endregion
    }
}