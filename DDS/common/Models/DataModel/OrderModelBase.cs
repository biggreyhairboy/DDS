using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.Data;
using OMS.common.Database;
using System.Threading;

namespace OMS.common.Models.DataModel
{
    public class OrderUpdateEventArgs : EventArgs
    {
        protected omsOrder order;

        public OrderUpdateEventArgs(omsOrder order)
        {
            this.order = order;
        }

        public omsOrder Order { get { return order; } }
    }

    public class SuborderUpdateEventArgs : EventArgs
    {
        protected amsOrder suborder;

        public SuborderUpdateEventArgs(amsOrder suborder)
        {
            this.suborder = suborder;
        }

        public amsOrder Suborder { get { return suborder; } }
    }

    public class TradeUpdateEventArgs : EventArgs
    {
        protected amsTrade trade;

        public TradeUpdateEventArgs(amsTrade trade)
        {
            this.trade = trade;
        }

        public amsTrade Trade { get { return trade; } }
    }

    public class CalculateUpdateEventArgs : OrderUpdateEventArgs
    {
        protected string msg;

        public CalculateUpdateEventArgs(omsOrder order)
            : base(order)
        { }

        public CalculateUpdateEventArgs(string msg)
            : base(new omsOrder(msg))
        {
            this.msg = msg;
        }

        public string CalculateMessage { get { return msg; } }
    }

    public delegate bool FilterHandler(object item);

    public class OrderModelBase : IDisposable
    {
        private static OrderModelBase instance;
        private static volatile object syncRoot = new object();

        public event EventHandler<OrderUpdateEventArgs> OnOrderUpdate;
        public event EventHandler<SuborderUpdateEventArgs> OnSuborderUpdate;
        public event EventHandler<TradeUpdateEventArgs> OnTradeUpdate;
        public event EventHandler<SuborderUpdateEventArgs> OnFirstSuborderUpdate;
        public event EventHandler<TradeUpdateEventArgs> OnFirstTradeUpdate;
        public event EventHandler<SuborderUpdateEventArgs> OnSuborderQueryDone;
        public event EventHandler<TradeUpdateEventArgs> OnTradeQueryDone;
        public event EventHandler<OrderUpdateEventArgs> OnErrorOrderUpdate;
        public event EventHandler<CalculateUpdateEventArgs> OnOrderCalculate;

        protected FilterHandler suborderTradeFilter;
        protected FilterHandler orderFilter;
        protected Dictionary<string, omsOrder> orders;
        protected Dictionary<string, amsOrder> suborders;
        protected Dictionary<string, amsOrder> orphanSuborders;
        protected Dictionary<string, amsTrade> trades;
        protected Dictionary<string, amsTrade> orphanTrades;
        protected bool recovering;
        protected bool loadTradeSuborder;
        protected List<string> users;
        protected SubscribeManager submgr;
        protected List<string> subscribedExchanges;
        protected bool isFirstSubOrderReceived = true;
        protected bool isFirstTradeReceived = true;

        public OrderModelBase()
        {
            orders = new Dictionary<string, omsOrder>();
            suborders = new Dictionary<string, amsOrder>();
            orphanSuborders = new Dictionary<string, amsOrder>();
            trades = new Dictionary<string, amsTrade>();
            orphanTrades = new Dictionary<string, amsTrade>();
            users = new List<string>();
            subscribedExchanges = new List<string>();
            loadTradeSuborder = true;
        }
        /// <summary>
        /// If not set, OrderModelBase will recover all the orders, otherwise, recover the specified users' orders only
        /// </summary>
        public List<string> LimitUsers { get { return users; } }
        /// <summary>
        /// Gets or sets value to indicate whether or not to load suborder and trade from database
        /// </summary>
        public bool LoadTradeSuborder { get { return loadTradeSuborder; } set { loadTradeSuborder = value; } }
        /// <summary>
        /// Gets or sets the subscribe manager for order model, if it's not set, default will take SubscribeManager.Instance
        /// </summary>
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
        /// <summary>
        /// Orders for internal usage
        /// </summary>
        internal Dictionary<string, omsOrder> Orders { get { return orders; } }
        /// <summary>
        /// Suborders for internal usage
        /// </summary>
        internal Dictionary<string, amsOrder> Suborders { get { return suborders; } }
        /// <summary>
        /// Trades for internal usage
        /// </summary>
        internal Dictionary<string, amsTrade> Trades { get { return trades; } }
        /// <summary>
        /// Reload for default database OMSDATA
        /// </summary>
        public virtual void ReloadFromDB()
        {
            ReloadFromDB("OMSDATA");
        }
        /// <summary>
        /// Reload data from database
        /// </summary>
        /// <param name="databaseAlias">Specify the database alias</param>
        public virtual void ReloadFromDB(string databaseAlias)
        {
            recovering = true;
            ReloadOrders(databaseAlias);
            if (loadTradeSuborder)
            {
                ReloadSuborders(databaseAlias);
                ReloadTrades(databaseAlias);
            }
            recovering = false;
        }
        /// <summary>
        /// Subscribe for Order, Suborder and Trade's update. In this case, the orders are ranged at the <seealso cref="LimitUsers"/>
        /// </summary>
        public virtual void SubscribeUpdate()
        {
            SubscribeOrder();
            SubscribeSuborderTrade();
        }
        /// <summary>
        /// Subscribe orders using <seealso cref="LimitUsers"/>
        /// </summary>
        public virtual void SubscribeOrder()
        {
            if (users.Count > 0)
            {
                foreach (string item in users)
                {
                    SubscribeListResult res = Submgr.SubscribeByList("ORD_" + item);
                    res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessOrders));
                }
            }
        }

        public virtual void SubscribeOrderByAccounts(List<string> accounts)
        {
            if (accounts != null && accounts.Count > 0)
            {
                foreach (string item in accounts)
                {
                    SubscribeListResult res = Submgr.SubscribeByList("ORDA_" + item);
                    res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessOrders));
                }
            }
        }

        public virtual void SubscribeOrderByExchanges(List<string> exchanges)
        {
            if (exchanges != null && exchanges.Count > 0)
            {
                foreach (string item in exchanges)
                {
                    SubscribeListResult res = Submgr.SubscribeByList("ORDX_" + item);
                    res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessOrders));
                }
            }
        }

        public virtual void RegisterSuborderTradeFilterRule(FilterHandler filter)
        {
            suborderTradeFilter = filter;
        }

        public virtual void RegisterOrderFilterRule(FilterHandler filter)
        {
            orderFilter = filter;
        }
        
        public virtual void SubscribeSuborderTrade()
        {
            SubscribeListResult res = Submgr.SubscribeByList("EXCHANGE");
            res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessExchangeCallback));
        }

        public virtual void SubscribeSuborderByExchange(string exch)
        {
            if (exch == null || exch.Trim() == "") return;
            SubscribeResult res = Submgr.SubscribeBySymbol("SUBORDER_" + exch);
            res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessSuborders));
        }

        public virtual void SubscribeTradeByExchange(string exch)
        {
            if (exch == null || exch.Trim() == "") return;
            SubscribeResult res = Submgr.SubscribeBySymbol("TRADE_" + exch);
            res.AddHandler(new EventHandler<SubscribeResultEventArgs>(ProcessTrades));
        }

        protected virtual void ProcessExchangeCallback(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                string exch = e.Result.GetAttributeAsString(omsConst.OMS_SYMBOL);
                if (exch == null || exch.Trim() == "") return;
                if (subscribedExchanges.IndexOf(exch) >= 0) return;
                subscribedExchanges.Add(exch);
                SubscribeSuborderByExchange(exch);
                SubscribeTradeByExchange(exch);
            }
        }

        protected virtual void ProcessOrders(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                omsOrder order = new omsOrder(e.Result.ToString());
                if (orderFilter != null)
                {
                    if (!orderFilter(order)) return;
                }
                AddOrder(order);
            }
        }

        protected virtual void ProcessSuborders(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                amsOrder suborder = new amsOrder(e.Result.ToString());
                if (suborderTradeFilter != null)
                {
                    if (!suborderTradeFilter(suborder)) return;
                }
                if (isFirstSubOrderReceived)
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Enter(syncRoot);
                    try
                    {
                        if (isFirstSubOrderReceived)
                        {
                            isFirstSubOrderReceived = false;
                            if (OnFirstSuborderUpdate != null)
                            {
                                if (omsCommon.SyncInvoker == null)
                                    OnFirstSuborderUpdate(this, new SuborderUpdateEventArgs(suborder));
                                else omsCommon.SyncInvoker.Invoke(OnFirstSuborderUpdate, new object[] { this, new SuborderUpdateEventArgs(suborder) });
                            }
                        }
                    }
                    finally
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Exit(syncRoot);
                    }
                }
                AddSuborder(suborder);
            }
        }

        protected virtual void ProcessTrades(object sender, SubscribeResultEventArgs e)
        {
            if (e.Result.IsValid)
            {
                amsTrade trade = new amsTrade(e.Result.ToString());
                if (suborderTradeFilter != null)
                {
                    if (!suborderTradeFilter(trade)) return;
                }
                if (isFirstTradeReceived)
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Enter(syncRoot);
                    try
                    {
                        if (isFirstTradeReceived)
                        {
                            isFirstTradeReceived = false;
                            if (OnFirstTradeUpdate != null)
                            {
                                if (omsCommon.SyncInvoker == null)
                                    OnFirstTradeUpdate(this, new TradeUpdateEventArgs(trade));
                                else omsCommon.SyncInvoker.Invoke(OnFirstTradeUpdate, new object[] { this, new TradeUpdateEventArgs(trade) });
                            }
                        }
                    }
                    finally
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Exit(syncRoot);
                    }
                }
                AddTrade(trade);
            }
        }

        protected string GetSqlQuotedUsers()
        {
            return GetSqlQuotedStrings(users);
        }

        protected string GetSqlQuotedStrings(List<string> items)
        {
            if (items == null) return "";
            if (items.Count == 0) return "";
            StringBuilder buffer = new StringBuilder();
            foreach (string item in items)
            {
                if (buffer.Length == 0) buffer.Append(string.Format("'{0}'", item));
                else buffer.Append(string.Format(",'{0}'", item));
            }
            return buffer.ToString();
        }

        public virtual void ReloadOrdersByAccounts(string alias, List<string> accounts)
        {
            string sql = "select * from omsTrade_File";
            if (accounts != null && accounts.Count > 0)
            {
                sql = string.Format("{0} where account in ({1})", sql, GetSqlQuotedStrings(accounts));
            }
            sql += " order by Order_No";
            ReloadOrders(alias, sql);
        }

        public virtual void ReloadSubordersByAccounts(string alias, List<string> accounts)
        {
            string sql = "select e.*, o.Order_Type from Exch_file e, OMSTrade_File o where e.Order_no = o.Order_No";
            if (accounts != null && accounts.Count > 0)
            {
                sql = string.Format("{0} and (e.Account in ({1}))", sql, GetSqlQuotedStrings(accounts));
            }
            sql += " order by e.Order_No, e.DateTime_Created";
            ReloadSuborders(alias, sql, false);
        }

        public virtual void ReloadTradesByAccounts(string alias, List<string> accounts)
        {
            string sql = "select t.*,e.Exchange from Trans_File t, Exch_File e where t.Exch_No=e.Exch_No";
            if (accounts != null && accounts.Count > 0)
            {
                sql = string.Format("{0} and t.account in ({1})", sql, GetSqlQuotedStrings(accounts));
            }
            sql += " order by t.DateTime_Created";
            ReloadTrades(alias, sql, false);
        }

        public virtual void ReloadSuborders(string databaseAlias)
        {
            string sql = "select e.*, o.Order_Type from Exch_file e, OMSTrade_File o where e.Order_no = o.Order_No";
            if (users.Count > 0)
            {
                sql = string.Format("{0} and (e.User_no in ({1}) or o.runner in ({1}))", sql, GetSqlQuotedUsers());
            }
            sql += " order by e.Order_No, e.DateTime_Created";
            ReloadSuborders(databaseAlias, sql, false);
        }

        public virtual void ReloadTrades(string databaseAlias)
        {
            string sql = "select t.*,s.Exchange from Trans_File t, Exch_File s where t.Exch_No=s.Exch_No";
            if (users.Count > 0)
            {
                sql = string.Format("{0} and (t.user_no in ({1}) or t.exch_no in (select e.Exch_No from Exch_File e, OMSTrade_File o where e.Order_No = o.Order_No and o.runner in ({1})))", sql, GetSqlQuotedUsers());
            }
            sql += " order by t.DateTime_Created";
            ReloadTrades(databaseAlias, sql, false);
        }

        public virtual void ReloadOrders(string databaseAlias)
        {
            string sql = "select * from omsTrade_File";
            if (users.Count > 0)
            {
                sql = string.Format("{0} where user_no in ({1}) or runner in ({1})", sql, GetSqlQuotedUsers());
            }
            sql += " order by Order_No";
            ReloadOrders(databaseAlias, sql);
        }

        public virtual void HealthCheckSuborders(string databaseAlias)
        {
            string sql = "select e.*, o.Order_Type from Exch_file e, OMSTrade_File o where e.Order_no = o.Order_No";
            if (users.Count > 0)
            {
                sql = string.Format("{0} and (e.User_no in ({1}) or o.runner in ({1}))", sql, GetSqlQuotedUsers());
            }
            sql += " order by e.Order_No, e.DateTime_Created";
            ReloadSuborders(databaseAlias, sql, true);
        }

        public virtual void HealthCheckSubordersByAccounts(string databaseAlias, List<string> accounts)
        {
            string sql = "select e.*, o.Order_Type from Exch_file e, OMSTrade_File o where e.Order_no = o.Order_No";
            if (accounts != null && accounts.Count > 0)
            {
                sql = string.Format("{0} and (e.Account in ({1}))", sql, GetSqlQuotedStrings(accounts));
            }
            sql += " order by e.Order_No, e.DateTime_Created";
            ReloadSuborders(databaseAlias, sql, true);
        }

        public virtual void HealthCheckTrades(string databaseAlias)
        {
            string sql = "select t.*,s.Exchange from Trans_File t, Exch_File s where t.Exch_No=s.Exch_No";
            if (users.Count > 0)
            {
                sql = string.Format("{0} and (t.user_no in ({1}) or t.exch_no in (select e.Exch_No from Exch_File e, OMSTrade_File o where e.Order_No = o.Order_No and o.runner in ({1})))", sql, GetSqlQuotedUsers());
            }
            sql += " order by t.DateTime_Created";
            ReloadTrades(databaseAlias, sql, true);
        }

        public virtual void HealthCheckTradesByAccounts(string databaseAlias, List<string> accounts)
        {
            string sql = "select t.*,e.Exchange from Trans_File t, Exch_File e where t.Exch_No=e.Exch_No";
            if (accounts != null && accounts.Count > 0)
            {
                sql = string.Format("{0} and t.account in ({1})", sql, GetSqlQuotedStrings(accounts));
            }
            sql += " order by t.DateTime_Created";
            ReloadTrades(databaseAlias, sql, true);
        }

        public virtual bool CheckIfSubOrderExistInDB(string databaseAlias,string subOrderNum)
        {
            if (subOrderNum == null || subOrderNum.Trim() == "")
                return false;
            string sql = "select * from Exch_File where Exch_No = '" + subOrderNum + "'";
            DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return true;
            return false;
        }

        public virtual bool CheckIfTradeExistInDB(string databaseAlias,string tradeNum)
        {
            if (tradeNum == null || tradeNum.Trim() == "")
                return false;
            string sql = "select * from Trans_File where Tran_No = '" + tradeNum + "'";
            DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return true;
            return false;
        }

        protected virtual void ReloadOrders(string alias, string sql)
        {
            DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(alias, sql);
            if (ds != null)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    omsOrder order = new omsOrder();
                    order.OrderNum = OmsHelper.GetStringFromRow(row, "Order_No");
                    order.Account = OmsHelper.GetStringFromRow(row, "Account");
                    order.OrderType = OmsHelper.GetIntFromRow(row, "Order_Type");
                    order.Symbol = OmsHelper.GetStringFromRow(row, "Symbol");
                    order.Quantity = OmsHelper.GetDecimalFromRow(row, "Quantity");
                    order.OpenClose = OmsHelper.GetIntFromRow(row, "Open_Close");
                    order.Price = OmsHelper.GetDecimalFromRow(row, "Limit_Price");
                    order.User = OmsHelper.GetStringFromRow(row, "User_No");
                    order.HedgeType = OmsHelper.GetIntFromRow(row, "Hedge");
                    order.PrincipalType = OmsHelper.GetIntFromRow(row, "Origin");
                    order.Filled = OmsHelper.GetDecimalFromRow(row, "Fill");
                    order.ShortSell = OmsHelper.GetIntFromRow(row, "Short_Sell");
                    order.Instruct = OmsHelper.GetIntFromRow(row, "Instruction");
                    order.Instruct1 = OmsHelper.GetIntFromRow(row, "Instruction1");
                    order.Instruct2 = OmsHelper.GetIntFromRow(row, "Instruction2");
                    order.Instruct3 = OmsHelper.GetIntFromRow(row, "Instruction3");
                    order.Instruct4 = OmsHelper.GetIntFromRow(row, "Instruction4");
                    order.Price1 = OmsHelper.GetDecimalFromRow(row, "Price1");
                    order.Price2 = OmsHelper.GetDecimalFromRow(row, "Price2");
                    order.Price3 = OmsHelper.GetDecimalFromRow(row, "Price3");
                    order.Price4 = OmsHelper.GetDecimalFromRow(row, "Price4");
                    order.Quantity1 = OmsHelper.GetDecimalFromRow(row, "quantity1");
                    order.Quantity2 = OmsHelper.GetDecimalFromRow(row, "quantity2");
                    order.Quantity3 = OmsHelper.GetDecimalFromRow(row, "quantity3");
                    order.Quantity4 = OmsHelper.GetDecimalFromRow(row, "quantity4");
                    order.OperatorFlag = OmsHelper.GetStringFromRow(row, "OperatorFlag");
                    order.Time = OmsHelper.GetDateTimeFromRow(row, "DateTime_Modified", "HH:mm:ss");
                    order.CreateTime = OmsHelper.GetDateTimeFromRow(row, "DateTime_Created", "HH:mm:ss");
                    order.ExpirationDate = OmsHelper.GetDateTimeFromRow(row, "ExpirationDate", "HH:mm:ss");
                    order.ValueDate = OmsHelper.GetDateTimeFromRow(row, "valueDate", "HH:mm:ss");
                    order.AvgPrice = OmsHelper.GetDecimalFromRow(row, "Ave_Price");
                    order.Working = OmsHelper.GetIntFromRow(row, "Working");
                    order.CreditChange = OmsHelper.GetDecimalFromRow(row, "CreditChange");
                    order.NetProceed = OmsHelper.GetDecimalFromRow(row, "netproceed");
                    order.UserRef = OmsHelper.GetStringFromRow(row, "reference");
                    order.Exch = OmsHelper.GetStringFromRow(row, "Exchange");
                    order.OrigOrderNum = OmsHelper.GetStringFromRow(row, "Original_no");
                    order.BasketNum = OmsHelper.GetStringFromRow(row, "BasketID");
                    order.WaveNum = OmsHelper.GetStringFromRow(row, "WaveID");
                    order.SystemRef = OmsHelper.GetStringFromRow(row, "SystemRef");
                    order.ApprovalChange = OmsHelper.GetDecimalFromRow(row, "ApprovalChange");
                    order.ApprovalApplied = OmsHelper.GetDecimalFromRow(row, "ApprovalApplied");
                    order.ApprovalID = OmsHelper.GetStringFromRow(row, "ApprovalID");
                    order.RejectID = OmsHelper.GetStringFromRow(row, "RejectID");
                    order.Msg = OmsHelper.GetStringFromRow(row, "Message");
                    order.Currency = OmsHelper.GetStringFromRow(row, "Currency");
                    order.Held = OmsHelper.GetDecimalFromRow(row, "held");
                    order.ReqStatus = OmsHelper.GetIntFromRow(row, "ReqStatus");
                    order.LastUser = OmsHelper.GetStringFromRow(row, "lastuser");
                    order.UserMsg = OmsHelper.GetStringFromRow(row, "UserMessage");
                    order.Desk = OmsHelper.GetStringFromRow(row, "desk");
                    order.CreditChange2 = OmsHelper.GetDecimalFromRow(row, "creditChange2");
                    order.ProductType = OmsHelper.GetIntFromRow(row, "prodtype");
                    order.Runner = OmsHelper.GetStringFromRow(row, "runner");
                    order.GTCInfo = OmsHelper.GetStringFromRow(row, "GTCInfo");
                    order.ParserCompSysRef(OmsHelper.GetStringFromRow(row, "CompSystemRef"));
                    order.Status = OmsHelper.GetOrderStatus(OmsHelper.GetStringFromRow(row, "Status"));
                    AddOrder(order);
                    TLog.DefaultInstance.WriteLog("ReloadOrders " + order.ToString(), LogType.INFO);
                }
            }
        }

        protected virtual void ReloadSuborders(string alias, string sql, bool healthCheck)
        {
            DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(alias, sql);
            if (ds != null)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    amsOrder suborder = new amsOrder();
                    suborder.OrderNum = OmsHelper.GetStringFromRow(row, "Order_No");
                    suborder.SuborderNum = OmsHelper.GetStringFromRow(row, "Exch_No");
                    suborder.Account = OmsHelper.GetStringFromRow(row, "Account");
                    suborder.Symbol = OmsHelper.GetStringFromRow(row, "Symbol");
                    suborder.Quantity = OmsHelper.GetDecimalFromRow(row, "Quantity");
                    suborder.Price = OmsHelper.GetDecimalFromRow(row, "Price");
                    suborder.User = OmsHelper.GetStringFromRow(row, "User_No");
                    suborder.HedgeType = OmsHelper.GetIntFromRow(row, "hedge");
                    suborder.PrincipalType = OmsHelper.GetIntFromRow(row, "Origin");
                    suborder.ShortSell = OmsHelper.GetIntFromRow(row, "Shortsell");
                    suborder.Time = OmsHelper.GetDateTimeFromRow(row, "DateTime_Modified", "HH:mm:ss");
                    if (suborder.Time == null || suborder.Time.Trim() == "")
                        suborder.Time = OmsHelper.GetDateTimeFromRow(row, "DateTime_Created", "HH:mm:ss");
                    suborder.Exch = OmsHelper.GetStringFromRow(row, "Exchange");
                    suborder.ExchDest = OmsHelper.GetStringFromRow(row, "ExchangeDest");
                    suborder.TradeType = OmsHelper.GetIntFromRow(row, "Trade_Type");
                    if (ds.Tables[0].Columns["Remark"] != null)
                        suborder.Msg = OmsHelper.GetStringFromRow(row, "Remark");
                    suborder.Status = OmsHelper.GetOrderStatus(OmsHelper.GetStringFromRow(row, "Status"));
                    if (!healthCheck)
                        AddSuborder(suborder);
                    else
                    {
                        bool hasExch = false;
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(suborders);
                        try
                        {
                            hasExch = HasSuborder(suborder.SuborderNum);
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(suborders);
                        }
                        if (!hasExch)
                            AddSuborder(suborder);
                    }
                    TLog.DefaultInstance.WriteLog("ReloadSuborders " + suborder.ToString(), LogType.INFO);
                }
            }
        }

        protected virtual void ReloadTrades(string alias, string sql, bool healthCheck)
        {
            DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(alias, sql);
            if (ds != null)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    amsTrade trade = new amsTrade();
                    trade.SuborderNum = OmsHelper.GetStringFromRow(row, "Exch_No");
                    trade.TradeNum = OmsHelper.GetStringFromRow(row, "Tran_no");
                    trade.Account = OmsHelper.GetStringFromRow(row, "Account");
                    trade.Symbol = OmsHelper.GetStringFromRow(row, "Symbol");
                    trade.Quantity = OmsHelper.GetDecimalFromRow(row, "Quantity");
                    trade.Price = OmsHelper.GetDecimalFromRow(row, "Trans_Price");
                    trade.User = OmsHelper.GetStringFromRow(row, "User_No");
                    trade.OrderType = OmsHelper.GetIntFromRow(row, "Order_Type");
                    trade.HedgeType = OmsHelper.GetIntFromRow(row, "Hedge");
                    trade.PrincipalType = OmsHelper.GetIntFromRow(row, "Origin");
                    trade.ShortSell = OmsHelper.GetIntFromRow(row, "Shortsell");
                    trade.TradeType = OmsHelper.GetIntFromRow(row, "Trade_Type");
                    trade.CounterParty = OmsHelper.GetStringFromRow(row, "BrokerID");
                    trade.Time = OmsHelper.GetDateTimeFromRow(row, "DateTime_Created", "HH:mm:ss");
                    trade.Status = OmsHelper.GetOrderStatus(OmsHelper.GetStringFromRow(row, "Status"));
                    if (!healthCheck)
                        AddTrade(trade);
                    else
                    {
                        bool hasTran = false;
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(trades);
                        try
                        {
                            hasTran = HasTrade(trade.TradeNum);
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(trades);
                        }
                        if (!hasTran) AddTrade(trade);
                    }
                    TLog.DefaultInstance.WriteLog("ReloadTrades " + trade.ToString(), LogType.INFO);
                }
            }
        }

        protected virtual void AddChildOrder(omsOrder order, omsOrder parentOrder)
        {
            //Assume atomic operation for "order"
            if (order == null) return;

            if (parentOrder != null)
            {
                if ((order.Instruct1 & OmsOrdConst.omsOrder1WorkChild) != 0)
                {
                    if (order.BasketNum != null && order.BasketNum.Trim() != "")
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(parentOrder);
                        try
                        {
                            parentOrder.AddChildWorkOrder(order);
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(parentOrder);
                        }
                    }
                }
            }
            //order.CalcAveragePrice();
        }

        public virtual void AddOrder(omsOrder order)
        {
            if (order == null) return;
            if (orderFilter != null)
            {
                if (!orderFilter(order)) return;
            }
            bool updateOrder = false;
            omsOrder oldOrder = null;
            omsOrder parentOrder = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(orders);
            try
            {
                updateOrder = HasOrder(order.OrderNum);
                if (updateOrder) oldOrder = OrderOf(order.OrderNum);
                else orders[order.OrderNum] = order;
                if (order.BasketNum != null && order.BasketNum.Trim() != "")
                {
                    if (HasOrder(order.BasketNum))
                        parentOrder = OrderOf(order.BasketNum);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(orders);
            }

            if (updateOrder)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(oldOrder);
                try
                {
                    omsOrder copyOldOrder = new omsOrder();
                    copyOldOrder.Assign(oldOrder);

                    if ((oldOrder.Quantity != order.Quantity) || (oldOrder.Price != order.Price) || (oldOrder.Status != order.Status) || (oldOrder.Msg != order.Msg))
                        oldOrder.DirtyFlag = true;
                    oldOrder.Assign(order);
                    oldOrder.Data = null;
                    oldOrder.Data = copyOldOrder;
                    oldOrder.DataState = OmsDataState.dsUpdated;

                    AddChildOrder(oldOrder, parentOrder);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(oldOrder);
                }
            }
            else
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(order);
                try
                {
                    order.DirtyFlag = true;
                    if (orphanSuborders.Count > 0)
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(orphanSuborders);
                        try
                        {
                            List<string> tmpsuborderNo = new List<string>();
                            foreach (amsOrder suborder in orphanSuborders.Values)
                            {
                                if (suborder.OrderNum == order.OrderNum)
                                {
                                    tmpsuborderNo.Add(suborder.SuborderNum);
                                    suborder.OrderType = order.OrderType;
                                    order.AddExchOrder(suborder);
                                }
                            }
                            foreach (string item in tmpsuborderNo)
                            {
                                orphanSuborders.Remove(item);
                            }
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(orphanSuborders);
                        }
                    }
                    AddChildOrder(order, parentOrder);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(order);
                }
            }

            if (updateOrder) FireOnOrderUpdate(oldOrder);
            else FireOnOrderUpdate(order);
        }

        public virtual void AddSuborder(amsOrder suborder)
        {
            if (suborder == null) return;
            if (suborderTradeFilter != null)
            {
                if (!suborderTradeFilter(suborder)) return;
            }
            bool updateSuborder = false;
            bool orderFind = false;
            omsOrder order = null;
            amsOrder oldSuborder = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(suborders);
            try
            {
                updateSuborder = HasSuborder(suborder.SuborderNum);
                if (updateSuborder) oldSuborder = SuborderOf(suborder.SuborderNum);
                else suborders[suborder.SuborderNum] = suborder;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(suborders);
            }

            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(orders);
            try
            {
                orderFind = HasOrder(suborder.OrderNum);
                if (orderFind) order = OrderOf(suborder.OrderNum);
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(orders);
            }

            if (updateSuborder)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(oldSuborder);
                try
                {
                    oldSuborder.Assign(suborder);
                    oldSuborder.DataState = OmsDataState.dsUpdated;
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(oldSuborder);
                }
            }
            else
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(suborder);
                try
                {
                    if (orphanTrades.Count > 0)
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(orphanTrades);
                        try
                        {
                            List<string> tmpTradeNo = new List<string>();
                            foreach (amsTrade trade in orphanTrades.Values)
                            {
                                if (trade.SuborderNum == suborder.SuborderNum)
                                {
                                    suborder.AddTrade(trade);
                                    tmpTradeNo.Add(trade.TradeNum);
                                }
                            }
                            foreach (string item in tmpTradeNo)
                            {
                                orphanTrades.Remove(item);
                            }
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(orphanTrades);
                        }
                    }
                    if (!orderFind)
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(orphanSuborders);
                        try
                        {
                            orphanSuborders[suborder.SuborderNum] = suborder;
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(orphanSuborders);
                        }
                    }
                    else
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(order);
                        try
                        {
                            order.AddExchOrder(suborder);
                            if (suborder.Trades.Count > 0)
                            {
                                suborder.TradeOpEx(order);
                            }
                            order.DirtyFlag = true;
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(order);
                        }
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(suborder);
                }
            }
            if (updateSuborder) FireOnSuborderUpdate(oldSuborder);
            else FireOnSuborderUpdate(suborder);
        }

        public virtual void AddTrade(amsTrade trade)
        {
            if (trade == null) return;
            if (suborderTradeFilter != null)
            {
                if (!suborderTradeFilter(trade)) return;
            }
            bool updateTrade = false;
            bool suborderFind = false;
            bool orderFind = false;
            omsOrder order = null;
            amsOrder suborder = null;
            amsTrade oldTrade = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(trades);
            try
            {
                updateTrade = HasTrade(trade.TradeNum);
                if (updateTrade) oldTrade = TradeOf(trade.TradeNum);
                else trades[trade.TradeNum] = trade;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(trades);
            }

            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(suborders);
            try
            {
                suborderFind = HasSuborder(trade.SuborderNum);
                if (suborderFind) suborder = SuborderOf(trade.SuborderNum);
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(suborders);
            }

            if (suborderFind)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(orders);
                try
                {
                    orderFind = HasOrder(suborder.OrderNum);
                    if (orderFind) order = OrderOf(suborder.OrderNum);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(orders);
                }
            }

            if (updateTrade)
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(oldTrade);
                try
                {
                    amsTrade copyOldTrade = new amsTrade();
                    copyOldTrade.Assign(oldTrade);
                    if (suborderFind)
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(suborder);
                        try
                        {
                            if (orderFind)
                            {
                                if (omsCommon.SyncInvoker == null)
                                    System.Threading.Monitor.Enter(order);
                                try
                                {
                                    order.DirtyFlag = true;
                                    if (oldTrade.OrderType == order.OrderType)
                                        order.ExecutedAmount -= oldTrade.Price * oldTrade.Quantity;
                                    else order.ExecutedAmount += oldTrade.Price * oldTrade.Quantity;
                                    if (trade.Status == omsConst.omsOrderFill)
                                    {
                                        if (trade.OrderType == order.OrderType)
                                            order.ExecutedAmount += trade.Price * trade.Quantity;
                                        else order.ExecutedAmount -= trade.Price * trade.Quantity;
                                    }
                                    trade.UpdateOrderFields(order);
                                }
                                finally
                                {
                                    if (omsCommon.SyncInvoker == null)
                                        System.Threading.Monitor.Exit(order);
                                }
                            }
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(suborder);
                        }
                    }
                    oldTrade.Assign(trade);
                    oldTrade.DataState = OmsDataState.dsUpdated;
                    oldTrade.Data = null;
                    oldTrade.Data = copyOldTrade;
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(oldTrade);
                }
            }
            else
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(trade);
                try
                {
                    if (suborderFind)
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(suborder);
                        try
                        {
                            suborder.AddTrade(trade);
                            if (orderFind)
                            {
                                if (omsCommon.SyncInvoker == null)
                                    System.Threading.Monitor.Enter(order);
                                try
                                {
                                    order.DirtyFlag = true;
                                    if (trade.Status == omsConst.omsOrderFill)
                                    {
                                        if (trade.OrderType == order.OrderType)
                                            order.ExecutedAmount += trade.Price * trade.Quantity;
                                        else order.ExecutedAmount -= trade.Price * trade.Quantity;
                                    }
                                    trade.UpdateOrderFields(order);
                                }
                                finally
                                {
                                    if (omsCommon.SyncInvoker == null)
                                        System.Threading.Monitor.Exit(order);
                                }
                            }
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(suborder);
                        }
                    }
                    else
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Enter(orphanTrades);
                        try
                        {
                            orphanTrades[trade.TradeNum] = trade;
                        }
                        finally
                        {
                            if (omsCommon.SyncInvoker == null)
                                System.Threading.Monitor.Exit(orphanTrades);
                        }
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(trade);
                }
            }
            if (updateTrade) FireOnTradeUpdate(oldTrade);
            else FireOnTradeUpdate(trade);
        }

        public virtual void AddErrorOrder(omsOrder order)
        {
            if (order == null) return;
            //just fire, no keeping
            FireOnErrorOrderUpdate(order);
        }

        public virtual void AddCalcOrder(omsOrder order)
        {
            if (order == null) return;
            //just fire, no keeping
            FireOnOrderCalculate(order);
        }

        public virtual void AddCalculateMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            FireOnOrderCalculate(msg);
        }

        public omsOrder OrderOf(string key)
        {
            Submgr.RequestVerify();
            if (key == null || key.Trim() == "") return null;
            if (orders.ContainsKey(key)) return orders[key];
            return null;
        }

        public amsOrder SuborderOf(string key)
        {
            Submgr.RequestVerify();
            if (key == null || key.Trim() == "") return null;
            if (suborders.ContainsKey(key)) return suborders[key];
            return null;
        }

        public amsTrade TradeOf(string key)
        {
            Submgr.RequestVerify();
            if (key == null || key.Trim() == "") return null;
            if (trades.ContainsKey(key)) return trades[key];
            return null;
        }

        public bool HasOrder(string key)
        {
            if (key == null || key.Trim() == "") return false;
            return orders.ContainsKey(key);
        }

        public bool HasSuborder(string key)
        {
            if (key == null || key.Trim() == "") return false;
            return suborders.ContainsKey(key);
        }

        public bool HasTrade(string key)
        {
            if (key == null || key.Trim() == "") return false;
            return trades.ContainsKey(key);
        }

        public void RemoveOrder(string key)
        {
            omsOrder order = OrderOf(key);
            if (order == null) return;
            orders.Remove(key);
            order = null;
        }

        public void RemoveSuborder(string key)
        {
            amsOrder suborder = SuborderOf(key);
            if (suborder == null) return;
            suborders.Remove(key);
            suborder = null;
        }

        public void RemoveTrade(string key)
        {
            amsTrade trade = TradeOf(key);
            if (trade == null) return;
            trades.Remove(key);
            trade = null;
        }

        public void QuerySuborderDone(amsOrder suborder)
        {
            FireOnSuborderQueryDone(suborder);
        }

        public void QueryTradeDone(amsTrade trade)
        {
            FireOnTradeQueryDone(trade);
        }

        public int OrderCount { get { return orders.Count; } }

        public int SuborderCount { get { return suborders.Count; } }

        public int TradeCount { get { return trades.Count; } }

        public bool Recovering { get { return recovering; } }

        public Dictionary<string, omsOrder>.KeyCollection.Enumerator OrderNumbers { get { return orders.Keys.GetEnumerator(); } }

        public Dictionary<string, amsOrder>.KeyCollection.Enumerator SuborderNumbers { get { return suborders.Keys.GetEnumerator(); } }

        public Dictionary<string, amsTrade>.KeyCollection.Enumerator TradeNumbers { get { return trades.Keys.GetEnumerator(); } }
        /// <summary>
        /// Acquire exclusive lock on orders
        /// </summary>
        public void AcquireOrderLock()
        {
            Monitor.Enter(orders);
        }
        /// <summary>
        /// Release an exclusive lock on orders
        /// </summary>
        public void ReleaseOrderLock()
        {
            Monitor.Exit(orders);
        }
        /// <summary>
        /// Acquire exclusive lock on suborders
        /// </summary>
        public void AcquireSuborderLock()
        {
            Monitor.Enter(suborders);
        }
        /// <summary>
        /// Release an exclusive lock on suborders
        /// </summary>
        public void ReleaseSuborderLock()
        {
            Monitor.Exit(suborders);
        }
        /// <summary>
        /// Acquire exclusive lock on trades
        /// </summary>
        public void AcquireTradeLock()
        {
            Monitor.Enter(trades);
        }
        /// <summary>
        /// Release an exclusive lock on trades
        /// </summary>
        public void ReleaseTradeLock()
        {
            Monitor.Exit(trades);
        }

        private void FireOnOrderUpdate(omsOrder order)
        {
            if (order == null) return;
            if (OnOrderUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnOrderUpdate(this, new OrderUpdateEventArgs(order));
                else omsCommon.SyncInvoker.Invoke(OnOrderUpdate, new object[] { this, new OrderUpdateEventArgs(order) });
            }
        }

        private void FireOnSuborderUpdate(amsOrder suborder)
        {
            if (suborder == null) return;
            if (OnSuborderUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnSuborderUpdate(this, new SuborderUpdateEventArgs(suborder));
                else omsCommon.SyncInvoker.Invoke(OnSuborderUpdate, new object[] { this, new SuborderUpdateEventArgs(suborder) });
            }
        }

        private void FireOnTradeUpdate(amsTrade trade)
        {
            if (trade == null) return;
            if (OnTradeUpdate != null)
            {
                bool IsLateTrade=false;

                amsOrder subOrder = SuborderOf(trade.SuborderNum);
                if (subOrder != null)
                {
                    omsOrder order = OrderOf(subOrder.OrderNum);
                    if (order != null)
                    {
                        IsLateTrade = ((order.Instruct2 & OmsOrdConst.omsOrder2LateTrade) > 0);
                    }
                    else
                    {
                        TLog.DefaultInstance.WriteLog("Cannot find order:" + subOrder.OrderNum, LogType.ERROR);
                    }
                }
                else
                {
                    TLog.DefaultInstance.WriteLog("Cannot find subOrder:" + trade.SuborderNum, LogType.ERROR);
                }
                                
                if (!IsLateTrade)
                {
                    if (omsCommon.SyncInvoker == null)
                        OnTradeUpdate(this, new TradeUpdateEventArgs(trade));
                    else omsCommon.SyncInvoker.Invoke(OnTradeUpdate, new object[] { this, new TradeUpdateEventArgs(trade) });
                }
            }
        }

        private void FireOnSuborderQueryDone(amsOrder suborder)
        {
            if (OnSuborderQueryDone != null)
            {
                if (suborderTradeFilter != null)
                {
                    if (!suborderTradeFilter(suborder)) return;
                }
                if (omsCommon.SyncInvoker == null)
                    OnSuborderQueryDone(this, new SuborderUpdateEventArgs(suborder));
                else omsCommon.SyncInvoker.Invoke(OnSuborderQueryDone, new object[] { this, new SuborderUpdateEventArgs(suborder) });
            }
        }

        private void FireOnTradeQueryDone(amsTrade trade)
        {
            if (OnTradeQueryDone != null)
            {
                if (suborderTradeFilter != null)
                {
                    if (!suborderTradeFilter(trade)) return;
                }
                if (omsCommon.SyncInvoker == null)
                    OnTradeQueryDone(this, new TradeUpdateEventArgs(trade));
                else omsCommon.SyncInvoker.Invoke(OnTradeQueryDone, new object[] { this, new TradeUpdateEventArgs(trade) });
            }
        }

        private void FireOnErrorOrderUpdate(omsOrder order)
        {
            if (order == null) return;
            if (OnErrorOrderUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnErrorOrderUpdate(this, new OrderUpdateEventArgs(order));
                else omsCommon.SyncInvoker.Invoke(OnErrorOrderUpdate, new object[] { this, new OrderUpdateEventArgs(order) });
            }
        }

        private void FireOnOrderCalculate(omsOrder order)
        {
            if (order == null) return;
            if (OnOrderCalculate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnOrderCalculate(this, new CalculateUpdateEventArgs(order));
                else omsCommon.SyncInvoker.Invoke(OnOrderCalculate, new object[] { this, new CalculateUpdateEventArgs(order) });
            }
        }

        private void FireOnOrderCalculate(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            if (OnOrderCalculate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnOrderCalculate(this, new CalculateUpdateEventArgs(msg));
                else omsCommon.SyncInvoker.Invoke(OnOrderCalculate, new object[] { this, new CalculateUpdateEventArgs(msg) });
            }
        }

        public static OrderModelBase Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new OrderModelBase();
                    }
                }
                return instance;
            }
        }

        public static void SetInstance(OrderModelBase item)
        {
            instance = item;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                OnOrderUpdate = null;
                OnSuborderUpdate = null;
                OnTradeUpdate = null;

                if (orders != null)
                {
                    orders.Clear();
                    orders = null;
                }
                if (suborders != null)
                {
                    suborders.Clear();
                    suborders = null;
                }
                if (trades != null)
                {
                    trades.Clear();
                    trades = null;
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion
    }
}
