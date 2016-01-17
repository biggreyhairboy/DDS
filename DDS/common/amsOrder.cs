using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.Text.RegularExpressions;

namespace OMS.common
{
    public class amsOrder
    {
        protected string orderNum;
        protected string suborderNum;
        protected string origOrderNum;
        protected string symbol;
        protected string account;
        protected string user;
        protected decimal price;
        protected decimal quantity;
        protected int orderType;
        protected int status;
        protected int instruct;
        protected int hedgeType;
        protected int principalType;
        protected int shortsell;
        protected string counterParty;
        protected string msg;
        protected decimal queued;
        protected decimal filled;
        protected decimal remain;
        protected decimal replaced;
        protected string time;
        protected string createdTime;
        protected decimal price1;
        protected decimal price2;
        protected decimal price3;
        protected decimal price4;
        protected decimal quantity1;
        protected decimal quantity2;
        protected decimal quantity3;
        protected decimal quantity4;
        protected int openclose;
        protected int errorCode;
        protected bool changePending;
        protected int changeQty;
        protected bool cancelPending;
        protected string exch;
        protected string exchDest;
        protected string userRef;
        protected Dictionary<string, amsTrade> trades;
        protected string basketNum;
        protected string waveNum;
        protected int tradeType;
        protected int settleType;
        protected bool setPrice;
        protected bool setQuantity;
        protected bool setPrice1;
        protected bool setPrice2;
        protected bool setPrice3;
        protected bool setPrice4;
        protected bool setQuantity1;
        protected bool setQuantity2;
        protected bool setQuantity3;
        protected bool setQuantity4;
        protected bool fixPrice;
        protected string systemRef;
        protected string cmd;
        protected string sessionID;
        protected int instruct1;
        protected int instruct2;
        protected int instruct3;
        protected int instruct4;
        protected bool setInstruct;
        protected int version;
        protected decimal creditChange;
        protected decimal creditChange2;
        protected string desk;
        protected object data;
        protected string sysRefOwner;
        protected string sysRefValue;
        protected List<string> compSysRef;
        protected Dictionary<string, string> compSystemReference;

        public amsOrder()
        {
            trades = new Dictionary<string, amsTrade>();
            compSystemReference = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            compSysRef = new List<string>();
            Reset();
        }

        public amsOrder(string msg)
            : this()
        {
            tradeType = -1;
            settleType = -1;
            version = 1;

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

        protected virtual void Reset()
        {
            orderNum = "";
            suborderNum = "";
            origOrderNum = "";
            symbol = "";
            account = "";
            user = "";
            counterParty = "";
            msg = "";
            time = "";
            createdTime = "";
            exch = "";
            exchDest = "";
            userRef = "";
            basketNum = "";
            waveNum = "";
            systemRef = "";
            cmd = "";
            sessionID = "";
            desk = "";
            sysRefOwner = "";
            sysRefValue = "";
            openclose = 0;
        }

        internal virtual void CopyTradesFrom(amsOrder order)
        {
            if (order == null) return;
            if (order.Trades == null) return;
            if (order.Trades.Count == 0) return;
            if (trades == null) trades = new Dictionary<string, amsTrade>();
            foreach (amsTrade item in order.Trades.Values)
            {
                AddTrade(item);
            }
        }

        protected virtual void UpdateField(string tag, string value)
        {
            int iTag = 0;
            if (int.TryParse(tag, out iTag))
            {
                switch (iTag)
                {
                    case omsConst.OMS_L_PRICE:
                        decimal.TryParse(value, out price);
                        setPrice = true;
                        break;
                    case omsConst.OMS_QUANTITY:
                        decimal.TryParse(value, out quantity);
                        setQuantity = true;
                        break;
                    case omsConst.OMS_ORDER_NO:
                        orderNum = value;
                        break;
                    case omsConst.OMS_EXCH_NO:
                        suborderNum = value;
                        break;
                    case omsConst.OMS_ACCOUNT:
                        account = value;
                        break;
                    case omsConst.OMS_USER_NO:
                        user = value;
                        break;
                    case omsConst.OMS_ORDER_TYPE:
                        int.TryParse(value, out orderType);
                        break;
                    case omsConst.OMS_SYMBOL:
                        symbol = value;
                        break;
                    case omsConst.OMS_STATUS:
                        int.TryParse(value, out status);
                        break;
                    case omsConst.OMS_FREE_TEXT:
                        msg = value;
                        break;
                    case omsConst.OMS_QUEUE_QTY:
                        decimal.TryParse(value, out queued);
                        break;
                    case omsConst.OMS_FILLQTY:
                        decimal.TryParse(value, out filled);
                        break;
                    case omsConst.OMS_REMQTY:
                        decimal.TryParse(value, out remain);
                        break;
                    case omsConst.OMS_NETCHANGE:
                        decimal.TryParse(value, out replaced);
                        break;
                    case omsConst.OMS_TIME:
                        time = value;
                        break;
                    case omsConst.OMS_INSTRUCT:
                        int.TryParse(value, out instruct);
                        setInstruct = true;
                        break;
                    case omsConst.OMS_INSTRUCT_1:
                        int.TryParse(value, out instruct1);
                        setInstruct = true;
                        break;
                    case omsConst.OMS_INSTRUCT_2:
                        int.TryParse(value, out instruct2);
                        setInstruct = true;
                        break;
                    case omsConst.OMS_INSTRUCT_3:
                        int.TryParse(value, out instruct3);
                        setInstruct = true;
                        break;
                    case omsConst.OMS_INSTRUCT_4:
                        int.TryParse(value, out instruct4);
                        setInstruct = true;
                        break;
                    case omsConst.OMS_PRICE_1:
                        decimal.TryParse(value, out price1);
                        setPrice1 = true;
                        break;
                    case omsConst.OMS_PRICE_2:
                        decimal.TryParse(value, out price2);
                        setPrice2 = true;
                        break;
                    case omsConst.OMS_PRICE_3:
                        decimal.TryParse(value, out price3);
                        setPrice3 = true;
                        break;
                    case omsConst.OMS_PRICE_4:
                        decimal.TryParse(value, out price4);
                        setPrice4 = true;
                        break;
                    case omsConst.OMS_QUANTITY_1:
                        decimal.TryParse(value, out quantity1);
                        setQuantity1 = true;
                        break;
                    case omsConst.OMS_QUANTITY_2:
                        decimal.TryParse(value, out quantity2);
                        setQuantity2 = true;
                        break;
                    case omsConst.OMS_QUANTITY_3:
                        decimal.TryParse(value, out quantity3);
                        setQuantity3 = true;
                        break;
                    case omsConst.OMS_QUANTITY_4:
                        decimal.TryParse(value, out quantity4);
                        setQuantity4 = true;
                        break;
                    case omsConst.OMS_ERRORCODE:
                        int.TryParse(value, out errorCode);
                        break;
                    case omsConst.OMS_ORIGINAL:
                        origOrderNum = value;
                        break;
                    case omsConst.OMS_COUNTERPARTY:
                        counterParty = value;
                        break;
                    case omsConst.OMS_HEDGE:
                        int.TryParse(value, out hedgeType);
                        break;
                    case omsConst.OMS_SHORTSELL:
                        int.TryParse(value, out shortsell);
                        break;
                    case omsConst.OMS_PRINCIPAL:
                        int.TryParse(value, out principalType);
                        break;
                    case omsConst.OMS_OPEN_CLOSE:
                        int.TryParse(value, out openclose);
                        break;
                    case omsConst.OMS_EXCHANGE:
                        exch = value;
                        break;
                    case omsConst.OMS_EXCHANGEDEST:
                        exchDest = value;
                        break;
                    case omsConst.OMS_CREATETIME:
                        createdTime = value;
                        break;
                    case omsConst.OMS_USER_REF:
                        userRef = value;
                        break;
                    case omsConst.OMS_BASKET_NO:
                        basketNum = value;
                        break;
                    case omsConst.OMS_WAVE_NO:
                        waveNum = value;
                        break;
                    case omsConst.OMS_TRADETYPE:
                        int.TryParse(value, out tradeType);
                        break;
                    case omsConst.OMS_SETTLETYPE:
                        int.TryParse(value, out settleType);
                        break;
                    case omsConst.OMS_SYSTEMREF:
                        systemRef = value;
                        break;
                    case omsConst.OMS_COMMAND:
                        cmd = value;
                        break;
                    case omsConst.OMS_SESSION_KEY:
                        sessionID = value;
                        break;
                    case omsConst.OMS_VERSION:
                        int.TryParse(value, out version);
                        break;
                    case omsConst.OMS_CREDIT:
                        decimal.TryParse(value, out creditChange);
                        break;
                    case omsConst.OMS_CREDIT2:
                        decimal.TryParse(value, out creditChange2);
                        break;
                    case omsConst.OMS_DESK:
                        desk = value;
                        break;
                    case omsConst.OMS_SYSREF_OWNER:
                        sysRefOwner = value;
                        break;
                    case omsConst.OMS_SYSREF_VALUE:
                        sysRefValue = value;
                        break;
                    case omsConst.OMS_COMP_SYSREF:
                        ParserCompSysRef(value);
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

        public void ParserCompSysRef(string value)
        {
            if (value == null || value.Trim() == "") return;
            Regex regex = new Regex(@"(?:^|,)(?:""((?>[^""]+|"""")*)""|([^"",]*))");
            Regex quoteRegex = new Regex("\"\"");
            Match m = regex.Match(value);
            if (m.Success)
            {
                while (m.Success)
                {
                    string item = "";
                    if (m.Groups[1].Success)
                    {
                        item = quoteRegex.Replace(m.Groups[1].Value, "\"");
                    }
                    else
                    {
                        item = m.Groups[2].Value;
                    }
                    if (item.Contains("="))
                    {
                        string[] pieces = item.Split('=');
                        if (pieces.Length == 2)
                        {
                            compSysRef.Add(item);
                            compSystemReference[pieces[0]] = pieces[1];
                        }
                    }

                    m = m.NextMatch();
                }
            }
            else compSysRef.Add(value);
        }

        public string CompSysRefOf(string key)
        {
            if (key == null) return null;
            lock (compSystemReference)
            {
                if (compSystemReference.ContainsKey(key))
                    return compSystemReference[key];
            }
            return null;
        }

        public void AddTrade(amsTrade trade)
        {
            if (trade == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(trades);
            try
            {
                trades[trade.TradeNum] = trade;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(trades);
            }
        }

        public void GetTradeAmount(ref decimal filledQty, ref decimal executedAmt)
        {
            filledQty = 0m;
            executedAmt = 0m;

            if (trades == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(trades);
            try
            {
                if (trades.Count == 0) return;
                foreach (amsTrade trade in trades.Values)
                {
                    if (trade.Status != omsConst.omsOrderFill) continue;
                    if (trade.OrderType == OrderType)
                    {
                        executedAmt += trade.Price * trade.Quantity;
                        filledQty += trade.Quantity;
                    }
                    else
                    {
                        executedAmt -= trade.Price * trade.Quantity;
                    }
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(trades);
            }
        }

        public void TradeOpEx(omsOrder order)
        {
            if (order == null) return;
            if (trades == null) return;

            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(trades);
            try
            {
                if (trades.Count == 0) return;
                foreach (amsTrade trade in trades.Values)
                {
                    if (trade.OrderType == order.OrderType)
                        order.ExecutedAmount += trade.Price * trade.Quantity;
                    else order.ExecutedAmount -= trade.Price * trade.Quantity;
                    trade.UpdateOrderFields(order);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(trades);
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            if (orderNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ORDER_NO, orderNum));
            if (suborderNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCH_NO, suborderNum));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SYMBOL, symbol));
            if (account.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ACCOUNT, account));
            if (orderType >= 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ORDER_TYPE, orderType));
            if (user.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_USER_NO, user));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY, quantity));
            buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_L_PRICE, price));
            if (origOrderNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ORIGINAL, origOrderNum));
            if (status != omsConst.omsOrderNull)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_STATUS, status));
            if (errorCode != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_ERRORCODE, errorCode));
            if (time.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TIME, time));
            if (createdTime.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_CREATETIME, createdTime));
            if (msg.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_FREE_TEXT, msg));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUEUE_QTY, queued));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_FILLQTY, filled));
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_REMQTY, remain));
            if (status == omsConst.omsOrderCancel)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_NETCHANGE, replaced));
            if (instruct != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_INSTRUCT, instruct));
            if (instruct1 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_INSTRUCT_1, instruct1));
            if (instruct2 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_INSTRUCT_2, instruct2));
            if (instruct3 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_INSTRUCT_3, instruct3));
            if (instruct4 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_INSTRUCT_4, instruct4));
            if (price1 != 0)
                buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_PRICE_1, price1));
            if (price2 != 0)
                buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_PRICE_2, price2));
            if (price3 != 0)
                buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_PRICE_3, price3));
            if (price4 != 0)
                buffer.Append(string.Format("{0}|{1:f6}|", omsConst.OMS_PRICE_4, price4));
            if (quantity1 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY_1, quantity1));
            if (quantity2 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY_2, quantity2));
            if (quantity3 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY_3, quantity3));
            if (quantity4 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_QUANTITY_4, quantity4));
            if (openclose != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_OPEN_CLOSE, openclose));
            if (hedgeType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_HEDGE, hedgeType));
            if (principalType != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_PRINCIPAL, principalType));
            if (shortsell != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SHORTSELL, shortsell));
            if (counterParty.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_COUNTERPARTY, counterParty));
            if (exch.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCHANGE, exch));
            if (exchDest.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_EXCHANGEDEST, exchDest));
            if (userRef.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_USER_REF, userRef));
            if (basketNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_BASKET_NO, basketNum));
            if (waveNum.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_WAVE_NO, waveNum));
            if (tradeType > -1)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_TRADETYPE, tradeType));
            if (settleType > -1)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SETTLETYPE, settleType));
            if (systemRef.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SYSTEMREF, systemRef));
            if (cmd.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_COMMAND, cmd));
            if (sessionID.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SESSION_KEY, sessionID));
            if (version != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_VERSION, version));
            if (creditChange != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_CREDIT, creditChange));
            if (creditChange2 != 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_CREDIT2, creditChange2));
            if (desk.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_DESK, desk));
            if (sysRefOwner.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SYSREF_OWNER, sysRefOwner));
            if (sysRefValue.Length > 0)
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_SYSREF_VALUE, sysRefValue));
            if (compSysRef.Count > 0)
            {
                StringBuilder csr = new StringBuilder();
                foreach (string item in compSysRef)
                {
                    if (csr.Length > 0)
                    {
                        csr.Append(string.Format(",{0}", item));
                    }
                    else
                    {
                        csr.Append(item);
                    }
                }
                buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_COMP_SYSREF, csr));
            }
            buffer.Append(string.Format("{0}|{1}|", omsConst.OMS_RESOURCE, "TRADE"));
            return buffer.ToString();
        }

        public virtual void Assign(amsOrder order)
        {
            if (order == null) return;
            this.account = order.Account;
            this.basketNum = order.BasketNum;
            this.cancelPending = order.CancelPending;
            this.changePending = order.ChangePending;
            this.changeQty = order.ChangeQuantity;
            this.cmd = order.Cmd;
            this.compSysRef = order.CompSysRef;
            this.counterParty = order.CounterParty;
            this.createdTime = order.CreateTime;
            this.creditChange = order.CreditChange;
            this.creditChange2 = order.CreditChange2;
            this.data = order.Data;
            this.desk = order.Desk;
            this.errorCode = order.ErrorCode;
            this.exch = order.Exch;
            this.exchDest = order.ExchDest;
            this.filled = order.Filled;
            this.fixPrice = order.FixPrice;
            this.hedgeType = order.HedgeType;
            this.instruct = order.Instruct;
            this.instruct1 = order.Instruct1;
            this.instruct2 = order.Instruct2;
            this.instruct3 = order.Instruct3;
            this.instruct4 = order.Instruct4;
            this.msg = order.Msg;
            this.openclose = order.OpenClose;
            this.orderNum = order.OrderNum;
            this.orderType = order.OrderType;
            this.origOrderNum = order.OrigOrderNum;
            this.price = order.Price;
            this.price1 = order.Price1;
            this.price2 = order.Price2;
            this.price3 = order.Price3;
            this.price4 = order.Price4;
            this.principalType = order.PrincipalType;
            this.quantity = order.Quantity;
            this.quantity1 = order.Quantity1;
            this.quantity2 = order.Quantity2;
            this.quantity3 = order.Quantity3;
            this.quantity4 = order.Quantity4;
            this.queued = order.Queued;
            this.remain = order.Remain;
            this.replaced = order.Replaced;
            this.sessionID = order.SessionID;
            this.setInstruct = order.setInstruct;
            this.setPrice = order.SetPrice;
            this.setPrice1 = order.SetPrice1;
            this.setPrice2 = order.SetPrice2;
            this.setPrice3 = order.SetPrice3;
            this.setPrice4 = order.SetPrice4;
            this.setQuantity = order.SetQuantity;
            this.setQuantity1 = order.SetQuantity1;
            this.setQuantity2 = order.SetQuantity2;
            this.setQuantity3 = order.SetQuantity3;
            this.setQuantity4 = order.SetQuantity4;
            this.settleType = order.SettleType;
            this.shortsell = order.ShortSell;
            this.status = order.Status;
            this.suborderNum = order.SuborderNum;
            this.symbol = order.Symbol;
            this.sysRefOwner = order.SysRefOwner;
            this.sysRefValue = order.SysRefValue;
            this.systemRef = order.SystemRef;
            this.time = order.Time;
            this.trades = order.Trades;
            this.tradeType = order.TradeType;
            this.user = order.User;
            this.userRef = order.UserRef;
            this.version = order.Version;
            this.waveNum = order.WaveNum;
            this.dataState = order.DataState;
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

        public bool IsOKCode
        {
            get
            {
                if ((errorCode == 0) || errorCode == OmsOrdConst.omsErrorOK) return true;
                return false;
            }
        }

        public string OrderNum { get { return orderNum; } set { orderNum = value; } }

        public string SuborderNum { get { return suborderNum; } set { suborderNum = value; } }

        public string OrigOrderNum { get { return origOrderNum; } set { origOrderNum = value; } }

        public string Symbol { get { return symbol; } set { symbol = value; } }

        public string Account { get { return account; } set { account = value; } }

        public string User { get { return user; } set { user = value; } }

        public decimal Price { get { return price; } set { price = value; } }
        /// <summary>
        /// Gets or sets the stop price
        /// </summary>
        public decimal Price1 { get { return price1; } set { price1 = value; } }
        /// <summary>
        /// Gets or sets the filled price
        /// </summary>
        public decimal Price2 { get { return price2; } set { price2 = value; } }

        public decimal Price3 { get { return price3; } set { price3 = value; } }

        public decimal Price4 { get { return price4; } set { price4 = value; } }

        public decimal Quantity { get { return quantity; } set { quantity = value; } }

        public decimal Quantity1 { get { return quantity1; } set { quantity1 = value; } }

        public decimal Quantity2 { get { return quantity2; } set { quantity2 = value; } }

        public decimal Quantity3 { get { return quantity3; } set { quantity3 = value; } }

        public decimal Quantity4 { get { return quantity4; } set { quantity4 = value; } }

        public int Instruct { get { return instruct; } set { instruct = value; } }

        public int Instruct1 { get { return instruct1; } set { instruct1 = value; } }

        public int Instruct2 { get { return instruct2; } set { instruct2 = value; } }

        public int Instruct3 { get { return instruct3; } set { instruct3 = value; } }

        public int Instruct4 { get { return instruct4; } set { instruct4 = value; } }

        public int OrderType { get { return orderType; } set { orderType = value; } }

        public int Status { get { return status; } set { status = value; } }

        public int HedgeType { get { return hedgeType; } set { hedgeType = value; } }

        public int PrincipalType { get { return principalType; } set { principalType = value; } }

        public int ShortSell { get { return shortsell; } set { shortsell = value; } }

        public string CounterParty { get { return counterParty; } set { counterParty = value; } }

        public string Msg { get { return msg; } set { msg = value; } }

        public decimal Queued { get { return queued; } set { queued = value; } }

        public decimal Filled { get { return filled; } set { filled = value; } }

        public decimal Remain { get { return remain; } set { remain = value; } }

        public decimal Replaced { get { return replaced; } set { replaced = value; } }

        public string Time { get { return time; } set { time = value; } }

        public string CreateTime { get { return createdTime; } set { createdTime = value; } }

        public int OpenClose { get { return openclose; } set { openclose = value; } }

        public int ErrorCode { get { return errorCode; } set { errorCode = value; } }

        public bool ChangePending { get { return changePending; } set { cancelPending = value; } }

        public int ChangeQuantity { get { return changeQty; } set { changeQty = value; } }

        public bool CancelPending { get { return cancelPending; } set { cancelPending = value; } }

        public string Exch { get { return exch; } set { exch = value; } }

        public string ExchDest { get { return exchDest; } set { exchDest = value; } }

        public string UserRef { get { return userRef; } set { userRef = value; } }

        public Dictionary<string, amsTrade> Trades { get { return trades; } set { trades = value; } }

        public string BasketNum { get { return basketNum; } set { basketNum = value; } }

        public string WaveNum { get { return waveNum; } set { waveNum = value; } }

        public int TradeType { get { return tradeType; } set { tradeType = value; } }

        public int SettleType { get { return settleType; } set { settleType = value; } }

        public bool SetPrice { get { return setPrice; } set { setPrice = value; } }

        public bool SetPrice1 { get { return setPrice1; } set { setPrice1 = value; } }

        public bool SetPrice2 { get { return setPrice2; } set { setPrice2 = value; } }

        public bool SetPrice3 { get { return setPrice3; } set { setPrice3 = value; } }

        public bool SetPrice4 { get { return setPrice4; } set { setPrice4 = value; } }

        public bool SetQuantity { get { return setQuantity; } set { setQuantity = value; } }

        public bool SetQuantity1 { get { return setQuantity1; } set { setQuantity1 = value; } }

        public bool SetQuantity2 { get { return setQuantity2; } set { setQuantity2 = value; } }

        public bool SetQuantity3 { get { return setQuantity3; } set { setQuantity3 = value; } }

        public bool SetQuantity4 { get { return setQuantity4; } set { setQuantity4 = value; } }

        public bool FixPrice { get { return fixPrice; } set { fixPrice = value; } }

        public string SystemRef { get { return systemRef; } set { systemRef = value; } }

        public string Cmd { get { return cmd; } set { cmd = value; } }

        public string SessionID { get { return sessionID; } set { sessionID = value; } }

        public int Version { get { return version; } set { version = value; } }

        public decimal CreditChange { get { return creditChange; } set { creditChange = value; } }

        public decimal CreditChange2 { get { return creditChange2; } set { creditChange2 = value; } }

        public string Desk { get { return desk; } set { desk = value; } }

        public object Data { get { return data; } internal set { data = value; } }

        public string SysRefOwner { get { return sysRefOwner; } set { sysRefOwner = value; } }

        public string SysRefValue { get { return sysRefValue; } set { sysRefValue = value; } }

        public List<string> CompSysRef { get { return compSysRef; } set { compSysRef = value; } }

        protected OmsDataState dataState = OmsDataState.dsNew;

        public OmsDataState DataState
        {
            get { return dataState; }
            internal set { dataState = value; }
        }
    }
}