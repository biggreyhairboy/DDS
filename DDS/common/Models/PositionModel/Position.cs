using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Models.AccountModel;
using OMS.common.Utilities;
using System.Threading;
using OMS.common.Models.DataModel;

namespace OMS.common.Models.PositionModel
{
    public class PositionUpdateEventArgs : EventArgs
    {
        private Position position;
        private UpdateType type;

        public PositionUpdateEventArgs(Position pos)
        {
            this.position = pos;
        }

        public Position Position { get { return position; } }

        public UpdateType UpdateType { get { return type; } set { type = value; } }
    }

    public class Position : IObserverPositionUpdate
    {
        public event EventHandler<PositionUpdateEventArgs> OnPositionUpdate;
        protected string account;
        protected AccountInfo accountInfo;
        protected Dictionary<string, PositionData> data;
        protected List<string> internetOPFlag;
        protected List<string> siOPFlag;
        protected Queue<string> symbolQueue;
        protected bool needPriceUpdate;

        protected volatile object syncRoot = new object();
        protected DateTime lastPriceTrigger;
        //protected AutoResetEvent priceReady;
        //protected ManualResetEvent priceDone;

        public Position(ISubjectPositionUpdate subject)
        {
            data = new Dictionary<string, PositionData>(StringComparer.InvariantCultureIgnoreCase);
            internetOPFlag = new List<string>();
            siOPFlag = new List<string>();
            siOPFlag.Add("S");
            symbolQueue = new Queue<string>();
            lastPriceTrigger = DateTime.Now;
            subject.RegisterPositionUpdate(this);
            //priceReady = new AutoResetEvent(false);
            //priceDone = new ManualResetEvent(false);
        }

        public PositionData this[string symbol]
        {
            get
            {
                if (symbol == null || symbol.Trim() == "") return null;
                if (data.ContainsKey(symbol)) return data[symbol];
                return null;
            }
        }
        /// <summary>
        /// Data represents all the positions for a specified account
        /// <para>
        /// Please DO NOT modify this property, if you need, please call function <seealso cref="AddPositionData"/>
        /// </para>
        /// </summary>
        public Dictionary<string, PositionData> Data { get { return data; } }
        /// <summary>
        /// Operator flags for Internet
        /// </summary>
        public List<string> InternetOperatorFlag { get { return internetOPFlag; } set { internetOPFlag = value; } }
        /// <summary>
        /// Operator flags for Vouchers, default with "S"
        /// </summary>
        public List<string> SIOperatorFlag { get { return siOPFlag; } set { siOPFlag = value; } }
        /// <summary>
        /// Gets or sets the value to indicate whether or not needs to listen the price update for symbols, default FALSE
        /// </summary>
        public bool NeedPriceUpdate
        {
            get { return needPriceUpdate; }
            set
            {
                if (needPriceUpdate != value)
                {
                    needPriceUpdate = value;
                    if (needPriceUpdate)
                    {
                        lastPriceTrigger = DateTime.Now;
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPriceUpdate));
                    }
                }
            }
        }

        //public void PriceDone()
        //{
        //    priceDone.Set();
        //}

        //private void ProcessPriceUpdate(object state)
        //{
        //    try
        //    {
        //        WaitHandle[] handles = new WaitHandle[] { priceReady, priceDone };
        //        Queue<string> workQueue = new Queue<string>();
        //        //loop...
        //        while (true)
        //        {
        //            if (WaitHandle.WaitAny(handles) == 1)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                lock (workQueue)
        //                {
        //                    workQueue.Clear();
        //                    lock (symbolQueue)
        //                    {
        //                        foreach (string symbol in symbolQueue)
        //                        {
        //                            workQueue.Enqueue(symbol);
        //                        }
        //                        symbolQueue.Clear();
        //                    }
        //                    if (workQueue.Count > 0)
        //                    {
        //                        foreach (string item in workQueue)
        //                        {
        //                            PositionData pvdata = data[item];
        //                            lock (pvdata)
        //                                pvdata.RecalcPL();
        //                        }
        //                        if (OnPositionUpdate != null)
        //                            OnPositionUpdate(this, new PositionUpdateEventArgs(this));
        //                    }
        //                }
        //                break;//Never hoop the working thread
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
        //    }
        //}

        protected virtual void PriceUpdate(object sender, PriceUpdateEventArgs e)
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(syncRoot);
            try
            {
                TimeSpan interval = DateTime.Now - lastPriceTrigger;
                //lock (symbolQueue)
                //{
                //    if (!symbolQueue.Contains(e.Price.Symbol))
                //        symbolQueue.Enqueue(e.Price.Symbol);//Here just need to record down which symbol's price changed, no need to add duplicate item
                //}
                if (interval.TotalSeconds >= omsCommon.PriceTriggerInterval)
                {
                    PriceUpdateInternal();
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(syncRoot);
            }
        }

        internal void PriceUpdateInternal()
        {
            //lock (priceReady)
            //{
            //    ThreadPool.QueueUserWorkItem(ProcessPriceUpdate);
            //    priceReady.Set();
            //    lastPriceTrigger = DateTime.Now;
            //}
            FireOnPositionUpdate(UpdateType.utPrice);
            lastPriceTrigger = DateTime.Now;
        }

        public virtual void MarginRatioUpdate(object sender, EventArgs e)
        {
            FireOnPositionUpdate(UpdateType.utPrice);
        }

        private void FireOnPositionUpdate(UpdateType type)
        {
            try
            {
                if (OnPositionUpdate != null)
                {
                    PositionUpdateEventArgs e = new PositionUpdateEventArgs(this);
                    e.UpdateType = type;
                    if (omsCommon.SyncInvoker == null)
                        OnPositionUpdate(this, e);
                    else if (!omsCommon.IsSyncInvokerDisposed)
                    {
                        omsCommon.SyncInvoker.Invoke(OnPositionUpdate, new object[] { this, e });
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public virtual void AddPositionData(PositionData item)
        {
            if (item == null) return;
            if (item.Symbol == null || item.Symbol.Trim() == "") return;
            if (needPriceUpdate)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(data);
                try
                {
                    if (!data.ContainsKey(item.Symbol))
                    {
                        item.Ticker.OnPriceUpdate += new EventHandler<PriceUpdateEventArgs>(PriceUpdate);
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(data);
                }
            }
            item.SIOperatorFlag = siOPFlag;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(data);
            try
            {
                data[item.Symbol] = item;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(data);
            }
        }

        public virtual void ReceiveTrade(amsTrade trade)
        {
            if (trade == null) return;
            if (trade.Account != account) return;
            PositionData posData = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(data);
            try
            {
                if (data.ContainsKey(trade.Symbol)) posData = data[trade.Symbol];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(data);
            }
            if (posData == null)
            {
                posData = new PositionData();
                posData.OnMarginRatioUpdate += new EventHandler<EventArgs>(MarginRatioUpdate);
                posData.Symbol = trade.Symbol;
                AddPositionData(posData);
            }
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(posData);
            try
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(trade);
                try
                {
                    posData.CalcPL(trade);
                    if ((trade.DataState == OmsDataState.dsNew) && (trade.Status != omsConst.omsOrderReject) && (trade.Status != omsConst.omsOrderCancel))
                    {
                        if ((trade.Price != 0) && (!CurrencyProcessor.Instance.IsCashSymbol(trade.Symbol)))
                            UpdateConfirmedTrans(posData, trade.Quantity * trade.Price, trade.Quantity, trade.OrderType, IsInternetTrade(trade.OperatorFlag), trade.OpenClose, trade.ShortSell, 1);
                    }
                    else if (((trade.DataState & OmsDataState.dsUpdated) != 0) && (trade.Status == omsConst.omsOrderFill))
                    {
                        amsTrade oldTrade = trade.Data as amsTrade;
                        if (oldTrade != null)
                        {
                            if ((trade.Price != 0) && (!CurrencyProcessor.Instance.IsCashSymbol(trade.Symbol)))
                                UpdateConfirmedTrans(posData, oldTrade.Quantity * oldTrade.Price, oldTrade.Quantity, oldTrade.OrderType, IsInternetTrade(oldTrade.OperatorFlag), oldTrade.OpenClose, oldTrade.ShortSell, -1);
                        }
                        if ((trade.Price != 0) && (!CurrencyProcessor.Instance.IsCashSymbol(trade.Symbol)))
                            UpdateConfirmedTrans(posData, trade.Quantity * trade.Price, trade.Quantity, trade.OrderType, IsInternetTrade(trade.OperatorFlag), trade.OpenClose, trade.ShortSell, 1);
                    }
                    else if ((trade.Status == omsConst.omsOrderReject) || (trade.Status == omsConst.omsOrderCancel))
                    {
                        if ((trade.Price != 0) && (!CurrencyProcessor.Instance.IsCashSymbol(trade.Symbol)))
                            UpdateConfirmedTrans(posData, trade.Quantity * trade.Price, trade.Quantity, trade.OrderType, IsInternetTrade(trade.OperatorFlag), trade.OpenClose, trade.ShortSell, -1);
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(trade);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(posData);
            }
            FireOnPositionUpdate(UpdateType.utTrade);
            if (omsCommon.LogDebugInfo)
                TLog.DefaultInstance.WriteLog(string.Format("DEBUG Quantity@ReceiveTrade Account:{0},TradeNum:{1},TradeQty:{2},OrderType:{3},Quantity",trade.Account,trade.TradeNum,trade.Quantity,trade.OrderType,posData.Quantity), LogType.INFO);
        }

        public virtual void ReceiveSuborder(amsOrder suborder)
        {
            //Do nothing
        }

        public virtual void ReceiveOrder(omsOrder order)
        {
            if (order == null) return;
            if ((order.Instruct1 & OmsOrdConst.omsOrder1WorkChild) != 0) return;
            PositionData pvdata = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(data);
            try
            {
                if (data.ContainsKey(order.Symbol)) pvdata = data[order.Symbol];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(data);
            }

            if (pvdata == null)
            {
                pvdata = new PositionData();
                pvdata.OnMarginRatioUpdate += new EventHandler<EventArgs>(MarginRatioUpdate);
                pvdata.Symbol = order.Symbol;
                AddPositionData(pvdata);
            }
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(pvdata);
            try
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(order);
                try
                {
                    if (order.DataState == OmsDataState.dsNew)
                    {
                        if (order.ErrorCode >= 0)
                        {
                            UpdatePositionData(pvdata, order, 1, true, false);
                        }
                    }
                    else
                    {
                        omsOrder lastOrder = order.Data as omsOrder;
                        if (lastOrder != null)
                        {
                            if ((order.Quantity != lastOrder.Quantity) || (order.Price != lastOrder.Price) || (order.Filled != lastOrder.Filled) ||
                                (order.Status != lastOrder.Status) || (order.CreditChange2 != lastOrder.CreditChange2) ||
                                ((order.ErrorCode != lastOrder.ErrorCode) && !(order.IsOKCode && lastOrder.IsOKCode)))
                            {
                                UpdatePositionData(pvdata, lastOrder, -1, false, true);
                                UpdatePositionData(pvdata, order, 1, false, false);
                            }
                        }
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(order);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(pvdata);
            }
            FireOnPositionUpdate(UpdateType.utOrder);
            if (omsCommon.LogDebugInfo)
                TLog.DefaultInstance.WriteLog(string.Format("DEBUG Quantity@ReceiveOrder Account:{0},OrderNum:{1},OrderQty:{2},OrdrType:{3},Quantity:{4}",order.Account,order.OrderNum,order.Quantity,order.OrderType,pvdata.Quantity), LogType.INFO);
        } 

        protected virtual bool IsInternetTrade(string opFlag)
        {
            if (opFlag != null && opFlag.Trim() != "")
            {
                if (internetOPFlag.Count > 0)
                {
                    if (internetOPFlag.IndexOf(opFlag) >= 0) return true;
                }
            }
            return false;
        }

        protected virtual void UpdatePositionData(PositionData pvdata, omsOrder order, int direction, bool isNewOrd, bool updateOld)
        {
            if (pvdata == null || order == null) return;
            if (pvdata.IsCurrency)
            {
                if (((order.Status == omsConst.omsOrderPending) || (order.Status == omsConst.omsOrderFill) || (order.Status == omsConst.omsOrderConfirm)) ||
                    ((order.Status == omsConst.omsOrderCancel) && ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0)))
                {
                    if ((order.Instruct & OmsOrdConst.omsOrderGTC) != 0)
                    {
                        UpdateGTCFund(pvdata, order, direction);
                    }
                    else if ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0)
                    {
                        if (order.Instruct1 == OmsOrdConst.omsOrder1UnClearCheck)
                        {
                            UpdateUnclearCheque(pvdata, order.CreditChange2, order.OrderType, direction);
                        }
                        else if ((order.Instruct1 & OmsOrdConst.omsOrder1TLVoucher) != 0)
                        {
                            UpdateTradeLimit(pvdata, order.CreditChange2, order.OrderType, direction);
                        }
                        else
                        {
                            UpdateFund(pvdata, order.CreditChange2, order.OrderType, direction);
                        }
                    }
                    else
                    {
                        if (order.Instruct1 == OmsOrdConst.omsOrder1UnClearCheck)
                        {
                            UpdateUnclearCheque(pvdata, order.Price, order.OrderType, direction);
                        }
                        else if ((order.Instruct1 & OmsOrdConst.omsOrder1TLVoucher) != 0)
                        {
                            UpdateTradeLimit(pvdata, order.Price, order.OrderType, direction);
                        }
                        else
                        {
                            UpdateFund(pvdata, order.Price, order.OrderType, direction);
                        }
                    }
                }
            }
            else
            {
                if ((order.Price == 0) && siOPFlag.Contains(order.OperatorFlag))
                {
                    if (((order.Status == omsConst.omsOrderPending) || (order.Status == omsConst.omsOrderFill) || (order.Status == omsConst.omsOrderConfirm)) ||
                        ((order.Status == omsConst.omsOrderCancel) && ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0)))
                    {
                        if ((order.Instruct & OmsOrdConst.omsOrderGTC) != 0)
                            UpdateGTCPosition(pvdata, order, direction);
                        else if ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0)
                            UpdatePosition(pvdata, Math.Round(order.CreditChange2), order.OrderType, direction);
                        else UpdatePosition(pvdata, order.Quantity, order.OrderType, direction);
                    }
                }
                else
                {
                    bool needUpdatePending = false;
                    if (isNewOrd)
                    {
                        if (((order.Status == omsConst.omsOrderNull) || (order.Status == omsConst.omsOrderPending) || (order.Status == omsConst.omsOrderPartialFill)) ||
                            ((order.Status == omsConst.omsOrderInactive) && ((order.Instruct4 & OmsOrdConst.omsOrder4ForceAddInactiveOrder) != 0)))
                            needUpdatePending = true;
                    }
                    else
                    {
                        /*if (updateOld) needUpdatePending = true;
                        else */if ((order.Status == omsConst.omsOrderNull) || (order.Status == omsConst.omsOrderPending) || (order.Status == omsConst.omsOrderPartialFill))
                            needUpdatePending = true;
                    }
                    if (needUpdatePending && ((order.Instruct2 & OmsOrdConst.omsOrder2LateTrade)==0))
                    {
                        decimal prc = order.Price;
                        if ((order.Instruct & OmsOrdConst.omsOrderPreOpen) != 0)
                        {
                            if (order.Price == 0)//Leo.W: If At-auction Order, its price should be ZERO, but if At-auction Limit Order, should keep using the order price
                                prc = order.Price1;
                        }
                        if (NeedUpdatePendingTrans(order))
                            UpdatePendingTrans(pvdata, (order.Quantity - order.Filled) * prc, (order.Quantity - order.Filled), order.OrderType, IsInternetTrade(order.OperatorFlag), order.OpenClose, order.ShortSell, direction);
                    }
                }
            }
        }

        protected virtual void UpdatePosition(PositionData pvdata, decimal qty, int orderType, int direction)
        {
            if (pvdata == null) return;
            if (orderType == omsConst.omsOrderSell)
            {
                pvdata.WithdrawQuantity += direction * qty;
                pvdata.Quantity -= direction * qty;
            }
            else if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.DepositQuantity += direction * qty;
                pvdata.Quantity += direction * qty;
            }
        }

        protected virtual void UpdateGTCPosition(PositionData pvdata, omsOrder order, int direction)
        {
            if (pvdata == null || order == null) return;
            if ((order.ExpirationDate == null || order.ExpirationDate.Trim() == "") || (order.ValueDate == null || order.ValueDate.Trim() == ""))
            {
                TLog.DefaultInstance.WriteLog(string.Format("GTC Deposit/Withdraw, but miss ExpirationDate or ValueDate for order [{0}]", order.OrderNum), LogType.ERROR);
                return;
            }
            decimal qty = order.Quantity;
            if ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0) qty = order.CreditChange2;
            DateTime valueDate = DateTime.Now;
            DateTime expireDate = DateTime.Now;
            bool validValueDate = DateTime.TryParse(order.ValueDate, out valueDate);
            bool validExpireDate = DateTime.TryParse(order.ExpirationDate, out expireDate);
            TimeSpan valueSpan = valueDate - DateTime.Now;
            TimeSpan expireSpan = expireDate - DateTime.Now;
            if (order.OrderType == omsConst.omsOrderSell)
            {
                if (validExpireDate && (expireSpan.Days <= 0))
                {
                    pvdata.GTCWithdrawQuantityWithdraw += direction * qty;
                    pvdata.GTCTradeQuantityWithdraw += direction * qty;
                    pvdata.Quantity -= direction * qty;
                }
                else if (validValueDate && (valueSpan.Days <= 0))
                {
                    pvdata.GTCWithdrawQuantityWithdraw += direction * qty;
                    pvdata.GTCTradeQuantityWithdraw += direction * qty;
                    pvdata.Quantity -= direction * qty;
                }
            }
            else if (order.OrderType == omsConst.omsOrderBuy)
            {
                if (validValueDate && valueSpan.Days <= 0)
                {
                    pvdata.GTCTradeQuautityDeposit += direction * qty;
                    pvdata.Quantity += direction * qty;
                }
                if (validExpireDate && expireSpan.Days <= 0)
                {
                    pvdata.GTCWithdrawQuantityDeposit += direction * qty;
                }
            }
        }

        protected virtual void UpdateFund(PositionData pvdata, decimal fund, int orderType, int direction)
        {
            if (pvdata == null) return;
            if (orderType == omsConst.omsOrderSell)
            {
                pvdata.TodaySellValue += direction * Math.Abs(fund);
                pvdata.CashDepositAmount += direction * Math.Abs(fund);
            }
            else if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.TodayBuyValue += direction * Math.Abs(fund);
                pvdata.CashWithdrawAmount += direction * Math.Abs(fund);
            }
        }

        protected virtual void UpdateTradeLimit(PositionData pvdata, decimal fund, int orderType, int direction)
        {
            if (pvdata == null) return;
            if (orderType == omsConst.omsOrderSell)
            {
                pvdata.TradeLimitDepositAmount += direction * Math.Abs(fund);
            }
            else if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.TradeLimitWithdrawAmount += direction * Math.Abs(fund);
            }
        }

        protected virtual void UpdateUnclearCheque(PositionData pvdata, decimal fund, int orderType, int direction)
        {
            if (pvdata == null) return;
            if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.UnclearChequeQuantity -= direction * Math.Abs(fund);
            }
            else if (orderType == omsConst.omsOrderSell)
            {
                pvdata.UnclearChequeQuantity += direction * Math.Abs(fund);
            }
        }

        protected virtual void UpdateGTCFund(PositionData pvdata, omsOrder order, int direction)
        {
            if (pvdata == null || order == null) return;
            if ((order.ExpirationDate == null || order.ExpirationDate.Trim() == "") || (order.ValueDate == null || order.ValueDate.Trim() == ""))
            {
                TLog.DefaultInstance.WriteLog(string.Format("GTC Deposit/Withdraw, but miss ExpirationDate or ValueDate for order [{0}]", order.OrderNum), LogType.ERROR);
                return;
            }
            decimal fund = order.Price;
            if ((order.Instruct & OmsOrdConst.omsOrderFAK) != 0) fund = Math.Abs(order.CreditChange2);
            DateTime valueDate = DateTime.Now;
            DateTime expireDate = DateTime.Now;
            bool validValueDate = DateTime.TryParse(order.ValueDate, out valueDate);
            bool validExpireDate = DateTime.TryParse(order.ExpirationDate, out expireDate);
            TimeSpan valueSpan = valueDate - DateTime.Now;
            TimeSpan expireSpan = expireDate - DateTime.Now;
            if (order.OrderType == omsConst.omsOrderSell)//Deposit
            {
                if (validValueDate)
                {
                    if (valueSpan.Days <= 0) pvdata.GTCTLDepositAmount += direction * fund;
                }
                if (validExpireDate)
                {
                    if (expireSpan.Days <= 0) pvdata.GTCCashDepositAmount += direction * fund;
                }
            }
            else if (order.OrderType == omsConst.omsOrderBuy)//Withdraw
            {
                if (validExpireDate && expireSpan.Days <= 0)
                {
                    pvdata.GTCCashWithdrawAmount += direction * fund;
                    pvdata.GTCTLWithdrawAmount += direction * fund;
                }
                else if (validValueDate && valueSpan.Days <= 0)
                {
                    pvdata.GTCCashWithdrawAmount += direction * fund;
                    pvdata.GTCTLWithdrawAmount += direction * fund;
                }
            }
        }

        protected virtual void UpdatePendingTrans(PositionData pvdata, decimal fund, decimal qty, int orderType, bool isInternet, int openClose, int shortSell, int direction)
        {
            if (pvdata == null) return;
            if (orderType == omsConst.omsOrderSell)
            {
                pvdata.PendingSellQuantity += qty * direction;
                pvdata.PendingSellValue += fund * direction;
                if (isInternet)
                {
                    pvdata.PendingISellQuantity += qty * direction;
                    pvdata.PendingISellValue += fund * direction;
                }
                if (openClose == 1)
                {
                    pvdata.PendingOpenShort += qty * direction;
                }
                else if (openClose == 2)
                {
                    pvdata.PendingCloseLong += qty * direction;
                }
            }
            else if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.PendingBuyQuantity += qty * direction;
                pvdata.PendingBuyValue += fund * direction;
                if (isInternet)
                {
                    pvdata.PendingIBuyQuantity += qty * direction;
                    pvdata.PendingIBuyValue += fund * direction;
                }
                if (openClose == 1)
                {
                    pvdata.PendingOpenLong += qty * direction;
                }
                else if (openClose == 2)
                {
                    pvdata.PendingCloseShort += qty * direction;
                }
            }
        }

        protected virtual void UpdateConfirmedTrans(PositionData pvdata, decimal fund, decimal qty, int orderType, bool isInternet, int openClose, int shortsell, int direction)
        {
            if (pvdata == null) return;
            bool isShortsell = false;
            if (shortsell != 0) isShortsell = true;
            if (orderType == omsConst.omsOrderBuy)
            {
                pvdata.Quantity += qty * direction;
                pvdata.TodayBuyQuantity += qty * direction;
                pvdata.TodayBuyValue += fund * direction;
                if (isInternet)
                {
                    pvdata.TodayIBuyQuantity += qty * direction;
                    pvdata.TodayIBuyValue += fund * direction;
                }
                if (openClose == 1)
                {
                    pvdata.TodayOpenLong += qty * direction;
                }
                else if (openClose == 2)
                {
                    pvdata.TodayCloseShort += qty * direction;
                }
            }
            else if (orderType == omsConst.omsOrderSell)
            {
                pvdata.Quantity -= qty * direction;
                pvdata.ShortQuantity += qty * direction;
                if (isShortsell)
                {
                    pvdata.ShortSellQuantity -= qty * direction;
                    pvdata.TodayShortSellValue += fund * direction;
                }
                pvdata.TodaySellQuantity += qty * direction;
                pvdata.TodaySellValue += fund * direction;
                if (isInternet)
                {
                    pvdata.TodayISellQuantity += qty * direction;
                    pvdata.TodayISellValue += fund * direction;
                }
                if (openClose == 1)
                {
                    pvdata.TodayOpenShort += qty * direction;
                }
                else if (openClose == 2)
                {
                    pvdata.TodayCloseLong += qty * direction;
                }
            }
        }

        private bool NeedUpdatePendingTrans(omsOrder order)
        {
            if (order == null) return false;
            if (order.BasketNum != null && order.BasketNum.Trim() != "")
            {
                if ((order.Instruct1 & OmsOrdConst.omsOrder1MultiBasket) != 0)
                {
                    if (order.WaveNum == "TEMPLATE") return false;
                }
                else if ((order.Instruct1 & OmsOrdConst.omsOrder1PercentBasket) != 0)
                {
                    if (order.WaveNum != "TEMPLATE") return false;
                }
            }
            return true;
        }

        public string Account
        {
            get { return account; }
            set
            {
                if (account != value)
                {
                    account = value;
                    if (AccountModelBase.Instance != null)
                        accountInfo = AccountModelBase.Instance.AccountOf(account);
                }
            }
        }

        public AccountInfo AccountInfo { get { return accountInfo; } }

        #region IObserverPositionUpdate Members

        public void FireUpdate()
        {
            PriceUpdate(this, null);
        }

        #endregion
    }
}
