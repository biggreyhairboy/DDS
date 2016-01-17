using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common.Models.AccountModel
{
    public class iAccountModel : AccountModelBase
    {
        protected IOmsITS its;

        public iAccountModel(IOmsITS its)
            : base()
        {
            this.its = its;
        }

        protected override void GetAccountInfo(string account)
        {
            its.QueryAccount(account);
        }
        /// <summary>
        /// Get account info using account ID
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <returns>AccountInfo</returns>
        /// <remarks>Attention this function will always return an <see cref="AccountInfo"/> object to the caller. If want to check the existing, please call <see cref="GetAccountInfoEx"/> instead</remarks>
        public override AccountInfo AccountOf(string account)
        {
            AccountInfo info = base.AccountOf(account);
            if (info == null && account != null && account.Trim() != "")
            {
                info = new AccountInfo(account);
                omsCommon.AcquireSyncLock(innerAccounts);
                try
                {
                    innerAccounts[info.Account] = info;
                }
                finally
                {
                    omsCommon.ReleaseSyncLock(innerAccounts);
                }
            }
            if (info.DataDynamic && (!JustCurrentAccount)) SubscribeAccountInfo(info);
            return info;
        }

        public AccountInfo AddAccount(string account)
        {
            if (account == null || account.Trim() == "") return null;
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                if (innerAccounts.ContainsKey(account)) return innerAccounts[account];
                AccountInfo info = new AccountInfo(account);
                innerAccounts[account] = info;
                return info;
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
        }

        public AccountInfo GetAccountInfoEx(string account)
        {
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                if (innerAccounts.ContainsKey(account)) return innerAccounts[account];
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
            return null;
        }

        public void ProcessAccountQuery(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            try
            {
                if (msg.Contains("|39|-2|")) return;//skip "Image not available" error
                if (msg.ToLower().StartsWith("queryaccount"))
                {
                    SubscribeResult sr = new SubscribeResult();
                    sr.ProcessMessage(string.Format("update|{0}", msg));
                    ProcessAccountUpdate(sr);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }
    }
}
