using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common
{
    public class amsTrade
    {
        protected string suborderNum;
        protected string tradeNum;
        protected string symbol;
        protected string account;
        protected string user;
        protected decimal price;
        protected decimal preClose;
        protected decimal quantity;
        protected int orderType;
        protected int hedgeType;
        protected int principalType;
        protected int shortsell;
        protected string counterParty;
        protected int tradeType;
        protected int settleType;
        protected int status;
        protected string msg;
        protected string time;
        protected string exch;
        protected string exchDest;
        protected string currency;
        protected string operatorFlag;
        protected int openClose;
        protected object data;

        public amsTrade()
        {
            Reset();
        }

        public amsTrade(string msg)
            : this()
        {
            OmsParser parser = new OmsParser(msg, "|");
            string tokenTag = "";
            string tokenValue = "";
            if (parser.Next(ref tokenTag))
            {
                while (parser.Next(ref tokenTag) && parser.Next(ref tokenValue))
                {
                    UpdateField(tokenTag, tokenValue);
                }
            }
        }

        public amsTrade(amsOrder order)
            : this()
        {
            suborderNum = order.SuborderNum;
            account = order.Account;
            user = order.User;
            symbol = order.Symbol;
            orderType = order.OrderType;
            quantity = order.Quantity;
            price = order.Price;
            openClose = order.OpenClose;
        }

        protected virtual void Reset()
        {
            suborderNum = "";
            tradeNum = "";
            symbol = "";
            account = "";
            user = "";
            counterParty = "";
            msg = "";
            time = "";
            exch = "";
            exchDest = "";
            currency = "";
            operatorFlag = "";
            openClose = 0;
        }

        public void UpdateOrderFields(omsOrder order)
        {
            if (order == null) return;
            currency = order.Currency;
            operatorFlag = order.OperatorFlag;
            openClose = order.OpenClose;
        }

        public virtual void UpdateField(string tag, string value)
        {
            int iTag = 0;
            if (int.TryParse(tag, out iTag))
            {
                switch (iTag)
                {
                    case omsConst.OMS_EXCH_NO:
                        suborderNum = value;
                        break;
                    case omsConst.OMS_TRAN_NO:
                        tradeNum = value;
                        break;
                    case omsConst.OMS_SYMBOL:
                        symbol = value;
                        break;
                    case omsConst.OMS_ACCOUNT:
                        account = value;
                        break;
                    case omsConst.OMS_ORDER_TYPE:
                        int.TryParse(value, out orderType);
                        break;
                    case omsConst.OMS_USER_NO:
                        user = value;
                        break;
                    case omsConst.OMS_QUANTITY:
                        decimal.TryParse(value, out quantity);
                        break;
                    case omsConst.OMS_L_PRICE:
                        decimal.TryParse(value, out price);
                        break;
                    case omsConst.OMS_STATUS:
                        int.TryParse(value, out status);
                        break;
                    case omsConst.OMS_HEDGE:
                        int.TryParse(value, out hedgeType);
                        break;
                    case omsConst.OMS_PRINCIPAL:
                        int.TryParse(value, out principalType);
                        break;
                    case omsConst.OMS_SHORTSELL:
                        int.TryParse(value, out shortsell);
                        break;
                    case omsConst.OMS_COUNTERPARTY:
                        counterParty = value;
                        break;
                    case omsConst.OMS_TRADETYPE:
                        int.TryParse(value, out tradeType);
                        break;
                    case omsConst.OMS_SETTLETYPE:
                        int.TryParse(value, out settleType);
                        break;
                    case omsConst.OMS_TIME:
                        time = value;
                        break;
                    case omsConst.OMS_EXCHANGE:
                        exch = value;
                        break;
                    case omsConst.OMS_EXCHANGEDEST:
                        exchDest = value;
                        break;
                    case omsConst.OMS_FREE_TEXT:
                        msg = value;
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

        public string StatusMessage
        {
            get
            {
                switch (status)
                {
                    case omsConst.omsOrderReject: return "Rejected";
                    case omsConst.omsOrderNull: return "";
                    case omsConst.omsOrderPending: return "Pending";
                    case omsConst.omsOrderPartialFill: return "Partial";
                    case omsConst.omsOrderFill: return "Complete";
                    case omsConst.omsOrderCancel: return "Cancel";
                    case omsConst.omsOrderInactive: return "Inactive";
                    case omsConst.omsOrderConfirm: return "Confirm";
                    default: return "Unknown";
                }
            }
        }

        public virtual void Assign(amsTrade trade)
        {
            if (trade == null) return;
            this.account = trade.Account;
            this.counterParty = trade.CounterParty;
            this.exch = trade.Exch;
            this.exchDest = trade.ExchDest;
            this.hedgeType = trade.HedgeType;
            this.msg = trade.Msg;
            this.orderType = trade.OrderType;
            this.price = trade.Price;
            this.principalType = trade.PrincipalType;
            this.quantity = trade.Quantity;
            this.settleType = trade.SettleType;
            this.shortsell = trade.ShortSell;
            this.status = trade.Status;
            this.suborderNum = trade.SuborderNum;
            this.symbol = trade.Symbol;
            this.time = trade.Time;
            this.tradeNum = trade.TradeNum;
            this.tradeType = trade.TradeType;
            this.user = trade.User;
            this.currency = trade.Currency;
            this.dataState = trade.DataState;
            this.operatorFlag = trade.OperatorFlag;
            this.data = trade.Data;
            this.preClose = trade.PreClose;
            this.openClose = trade.OpenClose;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCH_NO, suborderNum));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TRAN_NO, tradeNum));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SYMBOL, symbol));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ACCOUNT, account));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ORDER_TYPE, orderType));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_USER_NO, user));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY, quantity));
            buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_L_PRICE, price));
            if (status != omsConst.omsOrderNull)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_STATUS, status));
            if (hedgeType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_HEDGE, hedgeType));
            if (principalType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_PRINCIPAL, principalType));
            if (shortsell != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SHORTSELL, shortsell));
            if (counterParty.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_COUNTERPARTY, counterParty));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TRADETYPE, tradeType));
            if (settleType > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SETTLETYPE, settleType));
            if (time.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TIME, time));
            if (exch.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCHANGE, exch));
            if (exchDest.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCHANGEDEST, exchDest));
            if (msg.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_FREE_TEXT, msg));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_RESOURCE, "TRADE"));
            return buffer.ToString();
        }

        public int OpenClose { get { return openClose; } set { openClose = value; } }

        public decimal PreClose { get { return preClose; } set { preClose = value; } }

        public string OperatorFlag { get { return operatorFlag; } set { operatorFlag = value; } }

        public string SuborderNum { get { return suborderNum; } set { suborderNum = value; } }

        public string TradeNum { get { return tradeNum; } set { tradeNum = value; } }

        public string Symbol { get { return symbol; } set { symbol = value; } }

        public string Account { get { return account; } set { account = value; } }

        public string User { get { return user; } set { user = value; } }

        public decimal Price { get { return price; } set { price = value; } }

        public decimal Quantity { get { return quantity; } set { quantity = value; } }

        public int OrderType { get { return orderType; } set { orderType = value; } }

        public int HedgeType { get { return hedgeType; } set { hedgeType = value; } }

        public int PrincipalType { get { return principalType; } set { principalType = value; } }

        public int ShortSell { get { return shortsell; } set { shortsell = value; } }

        public string CounterParty { get { return counterParty; } set { counterParty = value; } }

        public int TradeType { get { return tradeType; } set { tradeType = value; } }

        public int SettleType { get { return settleType; } set { settleType = value; } }

        public int Status { get { return status; } set { status = value; } }

        public string Msg { get { return msg; } set { msg = value; } }

        public string Time { get { return time; } set { time = value; } }

        public string Exch { get { return exch; } set { exch = value; } }

        public string ExchDest { get { return exchDest; } set { exchDest = value; } }

        public string Currency { get { return currency; } set { currency = value; } }

        public object Data { get { return data; } set { data = value; } }

        protected OmsDataState dataState = OmsDataState.dsNew;

        public OmsDataState DataState
        {
            get { return dataState; }
            internal set { dataState = value; }
        }
    }

    public class amsCTFTrade : amsTrade
    { }

    public class amsCrossTrade : amsTrade
    { }
}