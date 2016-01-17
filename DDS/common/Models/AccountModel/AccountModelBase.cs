using System;
using System.Collections.Generic;
using OMS.common.Utilities;

namespace OMS.common.Models.AccountModel
{
    public class AccountUpdateEventArgs : EventArgs
    {
        protected AccountInfo account;

        public AccountUpdateEventArgs(AccountInfo account)
        {
            this.account = account;
        }

        public AccountInfo Account { get { return account; } }
    }

    public abstract class AccountModelBase : IDisposable
    {
        public event EventHandler<AccountUpdateEventArgs> OnAccountUpdate;

        private static AccountModelBase instance;

        protected Dictionary<string, AccountInfo> innerAccounts;
        protected Dictionary<string, List<string>> innerUsers;
        protected bool justCurrentAccount;
        protected SubscribeManager submgr;

        public AccountModelBase()
        {
            innerAccounts = new Dictionary<string, AccountInfo>(StringComparer.InvariantCultureIgnoreCase);
            innerUsers = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
            justCurrentAccount = false;
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

        public Dictionary<string, AccountInfo> Accounts { get { return innerAccounts; } }

        public void AcquireSyncLock()
        {
            omsCommon.AcquireSyncLock(innerAccounts);
        }

        public virtual AccountInfo SafeAccountOf(string account)
        {
            if (account != null && account.Trim() != "")
            {
                if (innerAccounts.ContainsKey(account)) return innerAccounts[account];
            }
            return null;
        }

        public void ReleaseSyncLock()
        {
            omsCommon.ReleaseSyncLock(innerAccounts);
        }

        protected abstract void GetAccountInfo(string account);

        public virtual AccountInfo AccountOf(string account)
        {
            Submgr.RequestVerify();
            if (account == null || account.Trim() == "") return null;
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                if (!innerAccounts.ContainsKey(account))
                    GetAccountInfo(account);
                if (innerAccounts.ContainsKey(account)) return innerAccounts[account];
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
            return null;
        }

        public virtual List<AccountInfo> AccountsOf(List<string> accounts)
        {
            return null;
        }

        public virtual void SubscribeAccountInfo(AccountInfo item)
        {
            if (item == null) return;
            SubscribeResult res = Submgr.SubscribeBySymbol("ACCT_" + item.Account);
            res.AddHandler(new EventHandler<SubscribeResultEventArgs>(AccountUpdate));
            item.DataDynamic = false;
        }

        public virtual void UnsubscribeAccountInfo()
        {
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                foreach (AccountInfo item in innerAccounts.Values)
                {
                    SubscribeResult res = Submgr.SubscribeBySymbol("ACCT_" + item.Account);
                    res.RemoveHandler(new EventHandler<SubscribeResultEventArgs>(AccountUpdate));
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
        }

        protected void AccountUpdate(object sender, SubscribeResultEventArgs e)
        {
            ProcessAccountUpdate(e.Result);
        }

        protected void ProcessAccountUpdate(SubscribeResult item)
        {
            if (item == null) return;
            if (!item.IsValid) return;

            TLog.DefaultInstance.WriteLog("DDS>|" + item.ToString(), LogType.INFO);
            string account = item.GetAttributeAsString(omsConst.OMS_ACCOUNT);
            if (account == null || account.Trim() == "") return;
            AccountInfo info = null;
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                if (!innerAccounts.ContainsKey(account)) return;
                info = innerAccounts[account];
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
            if (info == null) return;
            omsCommon.AcquireSyncLock(info);
            try
            {
                if (item.ContainsKey(omsConst.OMS_REQUIREDMARGIN))
                    info.HeldMargin = item.GetAttributeAsDecimal(omsConst.OMS_REQUIREDMARGIN);
                if (item.ContainsKey(omsConst.OMS_MNTMARGIN))
                    info.MaintainMargin = item.GetAttributeAsDecimal(omsConst.OMS_MNTMARGIN);
                if (item.ContainsKey(omsConst.OMS_PL))
                    info.PnL = item.GetAttributeAsDecimal(omsConst.OMS_PL);
                if (item.ContainsKey(omsConst.OMS_NET_EQUITY))
                    info.NetEquity = item.GetAttributeAsDecimal(omsConst.OMS_NET_EQUITY);
                if (item.ContainsKey(omsConst.OMS_MARGINCALL))
                    info.MarginCall = item.GetAttributeAsDecimal(omsConst.OMS_MARGINCALL);
                if (item.ContainsKey(omsConst.OMS_BOD_LOT_LIMIT))
                    info.BodLotLimit = item.GetAttributeAsDecimal(omsConst.OMS_BOD_LOT_LIMIT);
                if (item.ContainsKey(omsConst.OMS_LOT_LIMIT))
                    info.UsedLotLimit = item.GetAttributeAsDecimal(omsConst.OMS_LOT_LIMIT);
                if (item.ContainsKey(omsConst.OMS_FORTH_C_LIMIT))
                    info.ForthComeLimit = item.GetAttributeAsDecimal(omsConst.OMS_FORTH_C_LIMIT);
                if (item.ContainsKey(omsConst.OMS_SUSPEND_ACT))
                {
                    decimal tmp = item.GetAttributeAsDecimal(omsConst.OMS_SUSPEND_ACT);
                    info.IsSuspend = (tmp > 0);
                }
                if (item.ContainsKey(omsConst.OMS_WITHDRAWABLECASH_BAL))
                    info.WithdrawableBalance = item.GetAttributeAsDecimal(omsConst.OMS_WITHDRAWABLECASH_BAL);
                if (item.ContainsKey(omsConst.OMS_BOD_DAILY_TLIMIT))
                    info.InitialDailyTradingLimit = item.GetAttributeAsDecimal(omsConst.OMS_BOD_DAILY_TLIMIT);
                if (item.ContainsKey(omsConst.OMS_DAILY_TLIMIT))
                    info.AvailableDailyTradingLimit = item.GetAttributeAsDecimal(omsConst.OMS_DAILY_TLIMIT);
                if (item.ContainsKey(omsConst.OMS_TOTALCASHW))
                    info.CashWithdraw = item.GetAttributeAsDecimal(omsConst.OMS_TOTALCASHW);
                if (item.ContainsKey(omsConst.OMS_TOTALCASHD))
                    info.CashDeposit = item.GetAttributeAsDecimal(omsConst.OMS_TOTALCASHD);
                if (item.ContainsKey(omsConst.OMS_TOTAL_APPROVALPENDING))
                    info.PendingApproveAmt = item.GetAttributeAsDecimal(omsConst.OMS_TOTAL_APPROVALPENDING);
                if (item.ContainsKey(omsConst.OMS_QUANTITY_2))
                    info.SDCashBalance = item.GetAttributeAsDecimal(omsConst.OMS_QUANTITY_2);
                if (item.ContainsKey(omsConst.OMS_BOD_WEBTRADELIMIT))
                    info.BodWebTradingLimit = item.GetAttributeAsDecimal(omsConst.OMS_BOD_WEBTRADELIMIT);
                if (item.ContainsKey(omsConst.OMS_WEBTRADELIMIT))
                    info.WebTradingLimit = item.GetAttributeAsDecimal(omsConst.OMS_WEBTRADELIMIT);
                if (item.ContainsKey(omsConst.OMS_ACCT_AVAILABLEB))
                    info.ValiableBalance = item.GetAttributeAsDecimal(omsConst.OMS_ACCT_AVAILABLEB);
                if (item.ContainsKey(omsConst.OMS_ACCT_UNCLRCHEQUE))
                    info.UnclrChequeQty = item.GetAttributeAsDecimal(omsConst.OMS_ACCT_UNCLRCHEQUE);
                if (item.ContainsKey(omsConst.OMS_InTerestAccurl))
                    info.InterestAccrual = item.GetAttributeAsDecimal(omsConst.OMS_InTerestAccurl);
                if (item.ContainsKey(omsConst.OMS_InTerestAccurl_Date))
                    info.InterestAccrualDate = item.GetAttributeAsString(omsConst.OMS_InTerestAccurl_Date);
                if (item.ContainsKey(omsConst.OMS_CUTLOSS_VALUE))
                    info.CutLossValue = item.GetAttributeAsDecimal(omsConst.OMS_CUTLOSS_VALUE);
                if (item.ContainsKey(omsConst.OMS_INITMARGIN))
                    info.InitialMargin = item.GetAttributeAsDecimal(omsConst.OMS_INITMARGIN);
                if (item.ContainsKey(omsConst.OMS_REQUIREDMARGIN))
                    info.RequiredMargin = item.GetAttributeAsDecimal(omsConst.OMS_REQUIREDMARGIN);

                int bodlimitc = omsConst.OMS_BOD_TRADE_LIMIT;
                int limitc = omsConst.OMS_TRADE_LIMIT;
                int bodbalc = omsConst.OMS_BOD_CASH_BALANCE;
                int balc = omsConst.OMS_CASH_BALANCE;
                if (info.IsNewLimit)
                {
                    bodlimitc = omsConst.OMS_BOD_TRDFUND;
                    limitc = omsConst.OMS_TRDFUND;
                    bodbalc = omsConst.OMS_BOD_WITHDRAWFUND;
                    balc = omsConst.OMS_WITHDRAWFUND;
                }
                if (item.ContainsKey(bodlimitc))
                    info.BodTradingLimit = item.GetAttributeAsDecimal(bodlimitc);
                if (item.ContainsKey(limitc))
                    info.TradingLimit = item.GetAttributeAsDecimal(limitc);
                if (item.ContainsKey(bodbalc))
                    info.BodCashBalance = item.GetAttributeAsDecimal(bodbalc);
                if (item.ContainsKey(balc))
                    info.CashBalance = item.GetAttributeAsDecimal(balc);
                info.Dirty = true;
                FireAccountUpdate(info);
            }
            finally
            {
                omsCommon.ReleaseSyncLock(info);
            }
        }

        public virtual void RemoveAccount(string account)
        {
            if (account == null || account.Trim() == "") return;
            if (innerAccounts.ContainsKey(account))
            {
                omsCommon.AcquireSyncLock(innerAccounts);
                try
                {
                    AccountInfo info = innerAccounts[account];
                    innerAccounts.Remove(account);
                    if (innerUsers.ContainsKey(info.UserID))
                    {
                        omsCommon.AcquireSyncLock(innerUsers);
                        try
                        {
                            innerUsers.Remove(info.UserID);
                        }
                        finally
                        {
                            omsCommon.ReleaseSyncLock(innerUsers);
                        }
                    }
                }
                finally
                {
                    omsCommon.ReleaseSyncLock(innerAccounts);
                }
            }
        }

        public void FireAccountUpdate(AccountInfo info)
        {
            if (OnAccountUpdate != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnAccountUpdate(this, new AccountUpdateEventArgs(info));
                else omsCommon.SyncInvoker.Invoke(OnAccountUpdate, new object[] { this, new AccountUpdateEventArgs(info) });
            }
        }

        public bool JustCurrentAccount { get { return justCurrentAccount; } set { justCurrentAccount = value; } }

        public static AccountModelBase Instance
        {
            get { return instance; }
        }

        public static void SetInstance(AccountModelBase item)
        {
            instance = item;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (innerAccounts != null) innerAccounts.Clear();
            if (innerUsers != null) innerUsers.Clear();
        }

        #endregion
    }
}