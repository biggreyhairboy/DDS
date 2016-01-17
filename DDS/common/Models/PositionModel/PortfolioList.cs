using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using OMS.common.Utilities;
using OMS.common.Database;
using System.ComponentModel;

namespace OMS.common.Models.PositionModel
{
    public class PortfolioList : IDisposable, ISubjectPositionUpdate
    {
        public event EventHandler<PositionUpdateEventArgs> OnPortfolioUpdate;

        public delegate void OnTimerUpdateHandler();
        public event OnTimerUpdateHandler OnTimeUpdate;

        protected ISynchronizeInvoke syncInvoker;
        protected Dictionary<string, Position> bodPortfolioes;
        protected Dictionary<string, Position> portfolioes;//will contain the BOD Portfolio
        protected Dictionary<string, amsTrade> trades; //key: tradeNum
        protected bool needPriceUpdate;
        protected System.Threading.Timer timer;
        protected bool isTimerWorkingOnTick;

        public PortfolioList()
        {
            bodPortfolioes = new Dictionary<string, Position>(StringComparer.InvariantCultureIgnoreCase);
            portfolioes = new Dictionary<string, Position>(StringComparer.InvariantCultureIgnoreCase);
            trades = new Dictionary<string, amsTrade>(StringComparer.InvariantCultureIgnoreCase);
            if (omsCommon.PriceTriggerInterval > 0)
            {
                timer = new System.Threading.Timer(TimerOnTick);
                timer.Change(omsCommon.PriceTriggerInterval * 1000, omsCommon.PriceTriggerInterval * 1000);
                isTimerWorkingOnTick = false;
            }
        }

        public ISynchronizeInvoke SyncInvoker
        {
            get { return syncInvoker; }
            set { syncInvoker = value; }
        }
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
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Enter(portfolioes);
                    try
                    {
                        foreach (Position item in portfolioes.Values)
                        {
                            item.NeedPriceUpdate = needPriceUpdate;
                        }
                    }
                    finally
                    {
                        if (omsCommon.SyncInvoker == null)
                            System.Threading.Monitor.Exit(portfolioes);
                    }
                }
            }
        }

        private void TimerOnTick(object state)
        {
            if (OnTimeUpdate != null)
                OnTimeUpdate();
        }

        public Position CreatePortfolioByAccount(string account)
        {
            if (account == null || account.Trim() == "") return null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(account)) return portfolioes[account];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            Position pos = new Position(this);
            pos.NeedPriceUpdate = needPriceUpdate;
            pos.Account = account;
            pos.OnPositionUpdate += new EventHandler<PositionUpdateEventArgs>(pos_OnPositionUpdate);
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(bodPortfolioes);
            try
            {
                if (bodPortfolioes.ContainsKey(account))
                {
                    Position bodPos = bodPortfolioes[account];
                    foreach (string symbol in bodPos.Data.Keys)
                    {
                        pos.AddPositionData(bodPos.Data[symbol]);
                    }
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(bodPortfolioes);
            }
            portfolioes[account] = pos;
            return pos;
        }
        /// <summary>
        /// Add BOD Position to portfolio list
        /// </summary>
        /// <param name="item">BOD Position item</param>
        public void AddBodPosition(Position item)
        {
            if (item == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(bodPortfolioes);
            try
            {
                bodPortfolioes[item.Account] = item;
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(item.Data);
                try
                {
                    foreach (PositionData pvdata in item.Data.Values)
                    {
                        pvdata.InitBODQuantity(pvdata.BodQuantity);
                    }
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(item.Data);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(bodPortfolioes);
            }
        }
        /// <summary>
        /// Checks the position for a specified account
        /// </summary>
        /// <param name="account">Account to check</param>
        /// <returns>NULL if no position found, otherwise, return the corresponding position</returns>
        public Position PositionOf(string account)
        {
            if (account == null || account.Trim() == "") return null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(account)) return portfolioes[account];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            return null;
        }
        /// <summary>
        /// Checks the BOD position for a specified account
        /// </summary>
        /// <param name="account">Account to check</param>
        /// <returns>NULL if no BOD position found, otherwise, return the corresponding position</returns>
        public Position BodPositionOf(string account)
        {
            if (account == null || account.Trim() == "") return null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(bodPortfolioes);
            try
            {
                if (bodPortfolioes.ContainsKey(account)) return bodPortfolioes[account];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(bodPortfolioes);
            }
            return null;
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

        public void ReloadBODPosition()
        {
            ReloadBODPosition("OMSDATA", "Select * from BOD_Position_File order by Symbol");
        }

        public void ReloadBODPosition(List<string> accounts)
        {
            if (accounts == null) return;
            if (accounts.Count == 0) return;

            string sql = string.Format("Select * from BOD_Position_File where Account in ({0}) Order By Symbol", GetSqlQuotedStrings(accounts));
            ReloadBODPosition("OMSDATA", sql);
        }

        public void ReloadBODPosition(string dbAlias, string sql)
        {
            try
            {
                DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(dbAlias, sql);
                if (ds == null) return;
                List<string> bodAccounts = new List<string>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string account = OmsHelper.GetStringFromRow(row, "Account");
                    string symbol = OmsHelper.GetStringFromRow(row, "Symbol");
                    if (symbol == "" || account == "") continue;
                    Position bodData = null;
                    if (bodPortfolioes.ContainsKey(account)) bodData = bodPortfolioes[account];
                    else
                    {
                        bodData = new Position(this);
                        bodData.NeedPriceUpdate = needPriceUpdate;
                        bodData.Account = account;
                        bodData.OnPositionUpdate += new EventHandler<PositionUpdateEventArgs>(pos_OnPositionUpdate);
                        bodPortfolioes[account] = bodData;
                    }
                    PositionData symbolData = null;
                    if (bodData.Data.ContainsKey(symbol)) symbolData = bodData.Data[symbol];
                    else
                    {
                        symbolData = new PositionData();
                        symbolData.OnMarginRatioUpdate += new EventHandler<EventArgs>(bodData.MarginRatioUpdate);
                        symbolData.Symbol = symbol;
                        bodData.AddPositionData(symbolData);
                    }
                    symbolData.Quantity = OmsHelper.GetDecimalFromRow(row, "long_quantity");
                    symbolData.ShortQuantity = OmsHelper.GetDecimalFromRow(row, "short_quantity");
                    symbolData.AvgPrice = OmsHelper.GetDecimalFromRow(row, "Ave_price");
                    symbolData.BodQuantity = symbolData.Quantity;
                    symbolData.BodTradableQuantity = symbolData.Quantity;
                    symbolData.BodShortQuantity = symbolData.ShortQuantity;
                    symbolData.BodAvgPrice = symbolData.AvgPrice;
                    symbolData.InitBODQuantity(symbolData.Quantity);
                    bodAccounts.Add(account);
                    if (omsCommon.LogDebugInfo)
                        TLog.DefaultInstance.WriteLog(string.Format("DEBUG ReloadBODPosition Account:{0},LongQty:{1},ShortQty:{2},AvgPrice:{3}", account, symbolData.Quantity, symbolData.ShortQuantity, symbolData.AvgPrice), LogType.INFO);
                }
                if (bodAccounts.Count > 0)
                {
                    foreach (string account in bodAccounts)
                    {
                        CreatePortfolioByAccount(account);
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public void RemovePositionBy(string account)
        {
            if (account == null || account.Trim() == "") return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(account))
                    portfolioes.Remove(account);
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(bodPortfolioes);
            try
            {
                if (bodPortfolioes.ContainsKey(account))
                    bodPortfolioes.Remove(account);
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(bodPortfolioes);
            }
        }

        public void AddOrder(omsOrder order)
        {
            if (order == null) return;
            Position pos = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(order.Account))
                {
                    pos = portfolioes[order.Account];
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            if (pos == null)
                pos = CreatePortfolioByAccount(order.Account);
            pos.ReceiveOrder(order);
        }

        public void AddSuborder(amsOrder suborder)
        {
            if (suborder == null) return;
            Position pos = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(suborder.Account))
                    pos = portfolioes[suborder.Account];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            if (pos == null)
                pos = CreatePortfolioByAccount(suborder.Account);
            if (pos != null)
                pos.ReceiveSuborder(suborder);
            else
            {
                TLog.DefaultInstance.WriteLog(string.Format("Null position detected for suborder: {0}", suborder.ToString()), LogType.ERROR);
            }
        }

        public void AddTrade(amsTrade trade)
        {
            if (trade == null) return;
                        
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(trades);
            try
            {
                if ((trade.Status == omsConst.omsOrderReject) || (trade.Status == omsConst.omsOrderCancel))
                {
                    if (!trades.ContainsKey(trade.TradeNum))
                    {
                        TLog.DefaultInstance.WriteLog("Skip trade: " + trade.ToString(), LogType.INFO);
                        return;
                    }
                    else
                        trades.Remove(trade.TradeNum);
                }
                else   
                    trades[trade.TradeNum] = trade;    
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(trades);
            }

            Position pos = null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(portfolioes);
            try
            {
                if (portfolioes.ContainsKey(trade.Account))
                    pos = portfolioes[trade.Account];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(portfolioes);
            }
            if (pos == null)
                pos = CreatePortfolioByAccount(trade.Account);
            pos.ReceiveTrade(trade);
        }

        private void pos_OnPositionUpdate(object sender, PositionUpdateEventArgs e)
        {
            FirePortfolioUpdate(sender, e);
        }

        protected void FirePortfolioUpdate(object sender, PositionUpdateEventArgs e)
        {
            if (OnPortfolioUpdate != null)
            {
                if (syncInvoker != null)
                {
                    syncInvoker.Invoke(OnPortfolioUpdate, new object[] { sender, e });
                }
                else OnPortfolioUpdate(sender, e);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                //IsDisposed = true;
                if (timer != null)
                {
                    timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    timer.Dispose();
                    timer = null;
                }
                foreach (Position item in portfolioes.Values)
                {
                    //item.PriceDone();
                    UnregisterPositionUpdate(item as IObserverPositionUpdate);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion

        #region ISubjectPositionUpdate Members

        public void RegisterPositionUpdate(IObserverPositionUpdate item)
        {
            OnTimeUpdate += new OnTimerUpdateHandler(item.FireUpdate);
        }

        public void UnregisterPositionUpdate(IObserverPositionUpdate item)
        {
            OnTimeUpdate -= new OnTimerUpdateHandler(item.FireUpdate);
        }

        #endregion
    }
}
