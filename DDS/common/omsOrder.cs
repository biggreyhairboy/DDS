using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common
{
    public class omsOrder : amsOrder
    {
        protected Dictionary<string, amsOrder> exchOrders;
        protected Dictionary<string, omsOrder> childWorkOrders;
        protected OmsPrice ticker;
        protected bool allSent;
        protected int working;
        protected decimal addPending;
        protected bool cancelled;
        protected decimal creditApplied;
        protected decimal posChange;
        protected decimal posApplied;
        protected decimal netProceed;
        protected string operatorFlag;
        protected bool adjustQuantity;
        protected string currency;
        protected decimal held;
        protected decimal needClose;
        protected decimal posShorted;
        protected decimal approvalChange;
        protected decimal approvalApplied;
        protected decimal tradedQty;
        protected string approvalID;
        protected string rejectID;
        protected int validatedLevel;
        protected bool needApproval;
        protected string lastUser;
        protected string userMsg;
        protected decimal realizedAmount;
        protected decimal realizedAdjustment;
        protected decimal quantityChange;
        protected decimal priceChange;
        protected int minorCode;
        protected string expirationDate;
        protected decimal lastPrice;
        protected decimal lastQuantity;
        protected int lastStatus;
        protected string lastTradeNum;
        protected int lastProdType;
        protected string lastExeTime;
        protected string lastModifyTime;
        protected string runner;
        protected string gtcInfo;
        protected int reqStatus;
        protected omsOrder pendInfo;
        protected int prodType;
        protected bool dirtyFlag;
        protected decimal avgPrice;
        protected decimal executedAmount;
        protected string valueDate;

        //Fee & Charges
        protected decimal commission;
        protected decimal ccassFee;
        protected decimal stampDuty;
        protected decimal levy;
        protected decimal tradingFee;
        protected decimal tradeValue;
        protected decimal totalFee;

        public omsOrder()
            : base()
        {
            exchOrders = new Dictionary<string, amsOrder>();
            childWorkOrders = new Dictionary<string, omsOrder>();
        }
        
        public omsOrder(string msg)
            : base(msg)
        {
            exchOrders = new Dictionary<string, amsOrder>();
            childWorkOrders = new Dictionary<string, omsOrder>();
        }

        public void AddChildWorkOrder(omsOrder order)
        {
            if (order == null) return;
            if ((order.Instruct1 & OmsOrdConst.omsOrder1WorkChild) != 0)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(childWorkOrders);
                try
                {
                    childWorkOrders[order.OrderNum] = order;
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(childWorkOrders);
                }
                //if (order.DirtyFlag)
                //    CalcAveragePrice();
            }
        }

        public void CalcAveragePrice()
        {
            executedAmount = 0m;
            decimal filledQty = 0m;
            if ((instruct1 & OmsOrdConst.omsOrder1WorkParent) != 0)
            {
                if (childWorkOrders != null)
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Enter(childWorkOrders);
                    try
                    {
                        foreach (omsOrder order in childWorkOrders.Values)
                        {
                            CalcOrderPrice(ref executedAmount, ref filledQty, order.ExchOrders);
                        }
                    }
                    finally
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Exit(childWorkOrders);
                    }
                }
            }
            else
            {
                CalcOrderPrice(ref executedAmount, ref filledQty, exchOrders);
            }
            if (filledQty != filled)
                filled = filledQty;
            if (filled > 0)
                avgPrice = executedAmount / filled;
            else avgPrice = 0m;
        }

        protected void CalcOrderPrice(ref decimal executedAmt, ref decimal filledQty, Dictionary<string, amsOrder> suborders)
        {
            executedAmt = 0m;
            filledQty = 0m;
            if (suborders == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(suborders);
            try
            {
                foreach (amsOrder item in suborders.Values)
                {
                    decimal fq = 0m;
                    decimal ea = 0m;
                    item.GetTradeAmount(ref fq, ref ea);
                    executedAmt += ea;
                    filledQty += fq;
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(suborders);
            }
        }

        public bool AddExchOrder(amsOrder order)
        {
            if (order == null) return false;
            bool res = false;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(exchOrders);
            try
            {
                if (order.Exch == null || order.Exch.Trim() == "")
                    order.Exch = exch;
                if (!exchOrders.ContainsKey(order.SuborderNum))
                {
                    exchOrders[order.SuborderNum] = order;
                    if (order.OrigOrderNum == null || order.OrigOrderNum.Trim() == "")
                    {
                        if (addPending > 0)
                            addPending -= order.Quantity;
                        res = (order.Status == omsConst.omsOrderPending);
                    }
                }
                else
                {
                    amsOrder oldOrder = exchOrders[order.SuborderNum];
                    if ((!cancelled) && oldOrder.ChangePending && (order.Status == omsConst.omsOrderCancel) && (order.Replaced == 0))
                    {
                        adjustQuantity = true;
                    }
                    if ((!cancelled) && oldOrder.ChangePending && ((order.Status == omsConst.omsOrderPending) || (order.Status == omsConst.omsOrderPartialFill) || (order.Status == omsConst.omsOrderFill)) &&
                        ((order.Price != oldOrder.Price) || (order.Quantity != oldOrder.Quantity)))
                    {
                        res = true;
                    }
                    if ((oldOrder.ChangePending) && (order.Price == oldOrder.Price) && (order.Quantity == oldOrder.Quantity) && order.IsOKCode)
                    {
                        order.ChangeQuantity = oldOrder.ChangeQuantity;
                        order.ChangePending = oldOrder.ChangePending;
                    }
                    else if (oldOrder.CancelPending && (order.Status != omsConst.omsOrderCancel) && order.IsOKCode)
                    {
                        order.CancelPending = oldOrder.CancelPending;
                    }
                    else if ((oldOrder.ChangeQuantity > 0) && oldOrder.ChangePending)
                    {
                        addPending -= oldOrder.ChangeQuantity;
                    }
                    if ((order.Filled < oldOrder.Filled) && (order.Status != omsConst.omsOrderReject) && (order.Cmd != "correct") && (order.Status != omsConst.omsOrderCancel))
                        order.Filled = oldOrder.Filled;
                    order.CopyTradesFrom(oldOrder);
                    exchOrders[order.SuborderNum] = order;
                }

                if (order.OrigOrderNum != null && order.OrigOrderNum.Trim() != "")
                {
                    if (exchOrders.ContainsKey(order.OrigOrderNum))
                    {
                        amsOrder origOrder = exchOrders[order.OrigOrderNum];
                        if (origOrder != null && origOrder.ChangePending)
                        {
                            origOrder.ChangePending = false;
                            origOrder.Status = omsConst.omsOrderCancel;
                            if (origOrder.ChangeQuantity > 0)
                                addPending -= origOrder.ChangeQuantity;
                            decimal newFilled = origOrder.Quantity + origOrder.ChangeQuantity - order.Quantity;
                            if (newFilled > origOrder.Filled)
                                origOrder.Filled = newFilled;
                        }
                    }
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(exchOrders);
            }
            if (addPending < 0)
                addPending = 0;
            return res;
        }

        protected override void Reset()
        {
            base.Reset();

            operatorFlag = "";
            currency = "";
            approvalID = "";
            rejectID = "";
            lastUser = "";
            userMsg = "";
            expirationDate = "";
            lastTradeNum = "";
            lastExeTime = "";
            lastModifyTime = "";
            runner = "";
            gtcInfo = "";
            valueDate = "";
        }

        protected override void UpdateField(string tag, string value)
        {
            base.UpdateField(tag, value);
            int iTag = 0;
            if (int.TryParse(tag, out iTag))
            {
                switch (iTag)
                {
                    case omsConst.OMS_SYMBOL:
                        ticker = OmsPrice.PriceOf(symbol);
                        break;
                    case omsConst.OMS_WORKING:
                        int.TryParse(value, out working);
                        break;
                    case omsConst.OMS_OPERATORFLAG:
                        operatorFlag = value;
                        break;
                    case omsConst.OMS_CURRENCY:
                        currency = value;
                        break;
                    case omsConst.OMS_NETPROCEED:
                        decimal.TryParse(value, out netProceed);
                        break;
                    case omsConst.OMS_HELD:
                        decimal.TryParse(value, out held);
                        break;
                    case omsConst.OMS_APPROVALAPPLIED:
                        decimal.TryParse(value, out approvalApplied);
                        break;
                    case omsConst.OMS_APPROVALCHANGE:
                        decimal.TryParse(value, out approvalChange);
                        break;
                    case omsConst.OMS_APPROVALID:
                        approvalID = value;
                        break;
                    case omsConst.OMS_REJECTID:
                        rejectID = value;
                        break;
                    case omsConst.OMS_LASTUSER:
                        lastUser = value;
                        break;
                    case omsConst.OMS_VALIDATEDLEVEL:
                        int.TryParse(value, out validatedLevel);
                        break;
                    case omsConst.OMS_USRMSG:
                        userMsg = value;
                        break;
                    case omsConst.OMS_REALIZED_AMOUNT:
                        decimal.TryParse(value, out realizedAmount);
                        break;
                    case omsConst.OMS_REALIZED_ADJUST:
                        decimal.TryParse(value, out realizedAdjustment);
                        break;
                    case omsConst.OMS_MINORCODE:
                        int.TryParse(value, out minorCode);
                        break;
                    case omsConst.OMS_EXPIRATIONDATE:
                        expirationDate = value;
                        break;
                    case omsConst.OMS_LAST_PRICE:
                        decimal.TryParse(value, out lastPrice);
                        break;
                    case omsConst.OMS_LAST_QUANTITY:
                        decimal.TryParse(value, out lastQuantity);
                        break;
                    case omsConst.OMS_LAST_STATUS:
                        int.TryParse(value, out lastStatus);
                        break;
                    case omsConst.OMS_LAST_TRDNUM:
                        lastTradeNum = value;
                        break;
                    case omsConst.OMS_LAST_PTYPE:
                        int.TryParse(value, out lastProdType);
                        break;
                    case omsConst.OMS_LAST_EXETIME:
                        lastExeTime = value;
                        break;
                    case omsConst.OMS_REQ_STATUS:
                        int.TryParse(value, out reqStatus);
                        break;
                    case omsConst.OMS_PRODTYPE:
                        int.TryParse(value, out prodType);
                        break;
                    case omsConst.OMS_RUNNER:
                        runner = value;
                        break;
                    case omsConst.OMS_MODIFY_TIME:
                        lastModifyTime = value;
                        break;
                    case omsConst.OMS_GTC_INFO:
                        gtcInfo = value;
                        break;
                    case omsConst.OMS_TRADED_QTY:
                        decimal.TryParse(value, out tradedQty);
                        break;
                    case omsConst.OMS_OPEN_FOR_APPROVAL:
                        if (value == "1") needApproval = true;
                        else needApproval = false;
                        break;
                    case omsConst.OMS_VALUEDATE:
                        valueDate = value;
                        break;
                    case omsConst.OMS_COMMISSION:
                        decimal.TryParse(value, out commission);
                        break;
                    case omsConst.OMS_CCASS:
                        decimal.TryParse(value, out ccassFee);
                        break;
                    case omsConst.OMS_LEVY:
                        decimal.TryParse(value, out levy);
                        break;
                    case omsConst.OMS_STAMPDUTY:
                        decimal.TryParse(value, out stampDuty);
                        break;
                    case omsConst.OMS_TRADING_FEE:
                        decimal.TryParse(value, out tradingFee);
                        break;
                    case omsConst.OMS_TRDVALUE:
                        decimal.TryParse(value, out tradeValue);
                        break;
                    case omsConst.OMS_TOTAL:
                        decimal.TryParse(value, out totalFee);
                        break;
                    case omsConst.OMS_PRICE_2:
                        decimal.TryParse(value, out avgPrice);
                        break;
                    case omsConst.OMS_FILLQTY:
                        decimal.TryParse(value, out filled);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TLog.DefaultInstance.WriteLog("Invalid data [" + tag + ":" + value + "]", LogType.ERROR);
            }
        }

        public override void Assign(amsOrder order)
        {
            if (order == null) return;
            base.Assign(order);
            omsOrder item = order as omsOrder;
            if (item == null) return;
            exchOrders = item.ExchOrders;
            ticker = item.Ticker;
            allSent = item.AllSent;
            working = item.Working;
            creditApplied = item.CreditApplied;
            posChange = item.PosChange;
            posApplied = item.PosApplied;
            operatorFlag = item.OperatorFlag;
            currency = item.Currency;
            held = item.Held;
            approvalApplied = item.ApprovalApplied;
            approvalChange = item.ApprovalChange;
            approvalID = item.ApprovalID;
            rejectID = item.RejectID;
            lastUser = item.LastUser;
            validatedLevel = item.ValidatedLevel;
            needApproval = item.NeedApproval;
            userMsg = item.UserMsg;
            realizedAmount = item.RealizedAmount;
            realizedAdjustment = item.RealizedAdjustment;
            minorCode = item.MinorCode;
            expirationDate = item.ExpirationDate;
            lastPrice = item.LastPrice;
            lastQuantity = item.LastQuantity;
            lastStatus = item.LastStatus;
            lastTradeNum = item.LastTradeNum;
            lastProdType = item.LastProductType;
            lastExeTime = item.LastExeTime;
            reqStatus = item.ReqStatus;
            lastModifyTime = item.LastModifyTime;
            prodType = item.ProductType;
            runner = item.Runner;
            gtcInfo = item.GTCInfo;
            tradedQty = item.TradedQuantity;
            dirtyFlag = item.DirtyFlag;
            childWorkOrders = item.ChildWorkOrders;
            avgPrice = item.AvgPrice;
            executedAmount = item.ExecutedAmount;
            valueDate = item.ValueDate;
            commission = item.Commission;
            ccassFee = item.CCASSFee;
            stampDuty = item.StampDuty;
            levy = item.Levy;
            tradingFee = item.TradingFee;
            tradeValue = item.TradeValue;
            totalFee = item.TotalFee;
        }

        protected string InternetMessage()
        {
            string format = "working={0}, allSent={1},addPending={2},cancelled={3},posChange={4},posApplied={5},adjustQuantity={6},needClose={7},posShorted={8},needApproval={9},quantityChange={10},priceChange={11},netProceed={12}";
            string msg = string.Format(format, working, allSent ? 1 : 0, addPending, cancelled ? 1 : 0, posChange, posApplied, adjustQuantity ? 1 : 0, needClose, posShorted, needApproval ? 1 : 0, quantityChange, priceChange, netProceed);
            return msg;
        }

        public override string ToString()
        {
            string msg = base.ToString();
            StringBuilder buffer = new StringBuilder(msg);
            if (creditApplied != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_CREDITAPPLIED, creditApplied));
            if (netProceed != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_NETPROCEED, netProceed));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_WORKING, working));
            if (operatorFlag.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_OPERATORFLAG, operatorFlag));
            if (currency.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_CURRENCY, currency));
            if (held != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_HELD, held));
            if (approvalApplied != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_APPROVALAPPLIED, approvalApplied));
            if (approvalChange != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_APPROVALCHANGE, approvalChange));
            if (approvalID.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_APPROVALID, approvalID));
            if (rejectID.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_REJECTID, rejectID));
            if (lastUser.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LASTUSER, lastUser));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_VALIDATEDLEVEL, validatedLevel));
            if (userMsg.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_USRMSG, userMsg));
            if (realizedAmount != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_REALIZED_AMOUNT, realizedAmount));
            if (realizedAdjustment != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_REALIZED_ADJUST, realizedAdjustment));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_MINORCODE, minorCode));
            if (expirationDate.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXPIRATIONDATE, expirationDate));
            if (valueDate.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_VALUEDATE, valueDate));
            if (lastPrice > 0)
                buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_LAST_PRICE, lastPrice));
            if (lastQuantity > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LAST_QUANTITY, lastQuantity));
            if (lastStatus != omsConst.omsOrderNull)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LAST_STATUS, lastStatus));
            if (lastTradeNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LAST_TRDNUM, lastTradeNum));
            if (lastProdType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LAST_PTYPE, lastProdType));
            if (lastExeTime.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_LAST_EXETIME, lastExeTime));
            if (reqStatus != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_REQ_STATUS, reqStatus));
            if (prodType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_PRODTYPE, prodType));
            if (runner.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_RUNNER, runner));
            if (lastModifyTime.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_MODIFY_TIME, lastModifyTime));
            if (gtcInfo.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_GTC_INFO, gtcInfo));
            if (needApproval)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_OPEN_FOR_APPROVAL, "1"));
            if (tradedQty > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TRADED_QTY, tradedQty));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ORD_INTERNAL, InternetMessage()));
            return buffer.ToString();
        }

        public decimal AvgPrice { get { return avgPrice; } set { avgPrice = value; } }

        public decimal ExecutedAmount { get { return executedAmount; } set { executedAmount = value; } }

        public bool DirtyFlag { get { return dirtyFlag; } set { dirtyFlag = value; } }

        public Dictionary<string, amsOrder> ExchOrders { get { return exchOrders; } set { exchOrders = value; } }

        public Dictionary<string, omsOrder> ChildWorkOrders { get { return childWorkOrders; } set { childWorkOrders = value; } }

        public OmsPrice Ticker { get { return ticker; } set { ticker = value; } }

        public bool AllSent { get { return allSent; } set { allSent = value; } }

        public int Working { get { return working; } set { working = value; } }

        public decimal AddPending { get { return addPending; } set { addPending = value; } }

        public bool Cancelled { get { return cancelled; } set { cancelled = value; } }

        public decimal CreditApplied { get { return creditApplied; } set { creditApplied = value; } }

        public decimal PosChange { get { return posChange; } set { posChange = value; } }

        public decimal PosApplied { get { return posApplied; } set { posApplied = value; } }

        public decimal NetProceed { get { return netProceed; } set { netProceed = value; } }

        public string OperatorFlag { get { return operatorFlag; } set { operatorFlag = value; } }

        public bool AdjustQuantity { get { return adjustQuantity; } set { adjustQuantity = value; } }

        public string Currency { get { return currency; } set { currency = value; } }

        public decimal Held { get { return held; } set { held = value; } }

        public decimal NeedClose { get { return needClose; } set { needClose = value; } }

        public decimal PosShorted { get { return posShorted; } set { posShorted = value; } }

        public decimal ApprovalChange { get { return approvalChange; } set { approvalChange = value; } }

        public decimal ApprovalApplied { get { return approvalApplied; } set { approvalApplied = value; } }

        public string ApprovalID { get { return approvalID; } set { approvalID = value; } }

        public string RejectID { get { return rejectID; } set { rejectID = value; } }

        public int ValidatedLevel { get { return validatedLevel; } set { validatedLevel = value; } }

        public bool NeedApproval { get { return needApproval; } set { needApproval = value; } }

        public string LastUser { get { return lastUser; } set { lastUser = value; } }

        public string UserMsg { get { return userMsg; } set { userMsg = value; } }

        public decimal RealizedAmount { get { return realizedAmount; } set { realizedAmount = value; } }

        public decimal RealizedAdjustment { get { return realizedAdjustment; } set { realizedAdjustment = value; } }

        public decimal QuantityChange { get { return quantityChange; } set { quantityChange = value; } }

        public decimal PriceChange { get { return priceChange; } set { priceChange = value; } }

        public int MinorCode { get { return minorCode; } set { minorCode = value; } }

        public string ExpirationDate { get { return expirationDate; } set { expirationDate = value; } }

        public decimal LastPrice { get { return lastPrice; } set { lastPrice = value; } }

        public decimal LastQuantity { get { return lastQuantity; } set { lastQuantity = value; } }

        public int LastStatus { get { return lastStatus; } set { lastStatus = value; } }

        public string LastTradeNum { get { return lastTradeNum; } set { lastTradeNum = value; } }

        public int LastProductType { get { return lastProdType; } set { lastProdType = value; } }

        public string LastExeTime { get { return lastExeTime; } set { lastExeTime = value; } }

        public int ReqStatus { get { return reqStatus; } set { reqStatus = value; } }

        public omsOrder PendInfo { get { return pendInfo; } set { pendInfo = value; } }

        public int ProductType { get { return prodType; } set { prodType = value; } }

        public string Runner { get { return runner; } set { runner = value; } }

        public string LastModifyTime { get { return lastModifyTime; } set { lastModifyTime = value; } }

        public string GTCInfo { get { return gtcInfo; } set { gtcInfo = value; } }

        public decimal TradedQuantity { get { return tradedQty; } set { tradedQty = value; } }

        public string ValueDate { get { return valueDate; } set { valueDate = value; } }

        public decimal Commission { get { return commission; } set { commission = value; } }

        public decimal CCASSFee { get { return ccassFee; } set { ccassFee = value; } }

        public decimal StampDuty { get { return stampDuty; } set { stampDuty = value; } }

        public decimal Levy { get { return levy; } set { levy = value; } }

        public decimal TradeValue { get { return tradeValue; } set { tradeValue = value; } }

        public decimal TradingFee { get { return tradingFee; } set { tradingFee = value; } }

        public decimal TotalFee { get { return totalFee; } set { totalFee = value; } }
    }
}