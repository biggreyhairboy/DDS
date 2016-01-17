using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using OMS.common.Utilities;
using OMS.common.Database;
using OMS.common;
using System.Text;

namespace OMS.common.Models.AccountModel
{
    public class DBAccountModel : AccountModelBase
    {
        protected string defaultAccountQuery;
        protected string databaseAlias;

        public DBAccountModel(string databaseAlias)
            : base()
        {
            this.databaseAlias = databaseAlias;
            defaultAccountQuery = "select distinct c.*, b.limit as bodlimit, b.cash_Bal as bodCashBal,d.margin_type,d.Client_Title,d.Client_class,d.status,d.Customer_Type from customer_file c LEFT JOIN bod_customer_file b on b.account =  c.account LEFT JOIN customer_info d on d.account =c.account";
        }

        public void Reload(string sql)
        {
            LoadAccounts(sql);
        }

        public string AccountQueryStatement { get { return defaultAccountQuery; } set { defaultAccountQuery = value; } }

        protected void LoadAccount(string account)
        {
            string sql = defaultAccountQuery + " where c.account = '" + account + "'";
            LoadAccounts(sql);
        }

        protected void LoadAccounts(string sql)
        {
            if (sql == null || sql.Trim() == "") return;
            try
            {
                DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, sql);

                DataTable table = ds.Tables[0];
                foreach (DataRow row in table.Rows)
                {
                    string account = OmsHelper.GetStringFromRow(row, "Account");
                    AccountInfo info = null;
                    omsCommon.AcquireSyncLock(innerAccounts);
                    try
                    {
                        if (innerAccounts.ContainsKey(account))
                        {
                            info = innerAccounts[account];
                        }
                        else
                        {
                            info = new AccountInfo(account);
                            innerAccounts[account] = info;
                        }
                    }
                    finally
                    {
                        omsCommon.ReleaseSyncLock(innerAccounts);
                    }
                    omsCommon.AcquireSyncLock(info);
                    try
                    {
                        info.UserID = OmsHelper.GetStringFromRow(row, "User_No");
                        List<string> list = null;
                        omsCommon.AcquireSyncLock(innerUsers);
                        try
                        {
                            if (!innerUsers.ContainsKey(info.UserID))
                            {
                                list = new List<string>();
                                innerUsers[info.UserID] = list;
                            }
                            else list = innerUsers[info.UserID];
                        }
                        finally
                        {
                            omsCommon.ReleaseSyncLock(innerUsers);
                        }
                        if (list.IndexOf(account) < 0) list.Add(account);
                        info.FirstName = OmsHelper.GetStringFromRow(row, "First_Name");
                        info.LastName = OmsHelper.GetStringFromRow(row, "Family_Name");
                        info.ChineseName = OmsHelper.GetStringFromRow(row, "Chn_Name");
                        info.AccountName = OmsHelper.GetStringFromRow(row, "Account_Name");
                        info.DayPhone = OmsHelper.GetStringFromRow(row, "Day_Tel_1");
                        info.HomePhone = OmsHelper.GetStringFromRow(row, "Home_Tel");
                        info.Mobile = OmsHelper.GetStringFromRow(row, "Mobile");
                        info.Fax = OmsHelper.GetStringFromRow(row, "Fax");
                        info.eMail = OmsHelper.GetStringFromRow(row, "E_Mail");
                        info.BodTradingLimit = OmsHelper.GetDecimalFromRow(row, "bodlimit");
                        info.TradingLimit = OmsHelper.GetDecimalFromRow(row, "limit");
                        info.BodCashBalance = OmsHelper.GetDecimalFromRow(row, "bodCashBal");
                        info.CashBalance = OmsHelper.GetDecimalFromRow(row, "Cash_Bal");
                        info.MarginType = OmsHelper.GetIntFromRow(row, "margin_type");
                        info.RoleType = OmsHelper.GetIntFromRow(row, "RoleType");
                        info.ClientClass = OmsHelper.GetStringFromRow(row, "Client_Class");
                        info.ClientTitle = OmsHelper.GetStringFromRow(row, "Client_Title");
                        info.IsSuspend = !OmsHelper.GetBooleanFromRow(row, "Active");
                        info.MasterAccount = OmsHelper.GetStringFromRow(row, "Master");
                        info.Password = OmsHelper.GetStringFromRow(row, "Password");
                        info.Currency = OmsHelper.GetStringFromRow(row, "Currency");
                        info.Entitlement = OmsHelper.GetStringFromRow(row, "Entitlement");
                        info.Gender = OmsHelper.GetStringFromRow(row, "Gender");
                        info.NationalID = OmsHelper.GetStringFromRow(row, "National_ID");
                        info.HoldAmount = OmsHelper.GetDecimalFromRow(row, "HoldAmount");
                        info.CustomerType = OmsHelper.GetStringFromRow(row, "Customer_Type");
                        info.WebTradingLimit = OmsHelper.GetDecimalFromRow(row, "WebLimit");
                        info.Dirty = true;
                        if (info.DataDynamic && (!JustCurrentAccount))
                            SubscribeAccountInfo(info);
                    }
                    finally
                    {
                        omsCommon.ReleaseSyncLock(info);
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        protected void LoadAccountsTradingLimitGross(string sql)
        {
            if (sql == null || sql.Trim() == "") return;
            try
            {
                DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, sql);

                DataTable table = ds.Tables[0];
                foreach (DataRow row in table.Rows)
                {
                    string account = OmsHelper.GetStringFromRow(row, "AccountID");
                    decimal tradingLimitGross = OmsHelper.GetDecimalFromRow(row, "Gross");
                    omsCommon.AcquireSyncLock(innerAccounts);
                    try
                    {
                        if (innerAccounts.ContainsKey(account))
                            innerAccounts[account].TradingLimitGross = tradingLimitGross;
                    }
                    finally
                    {
                        omsCommon.ReleaseSyncLock(innerAccounts);
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        protected override void GetAccountInfo(string account)
        {
            LoadAccount(account);
        }

        public override List<AccountInfo> AccountsOf(List<string> accounts)
        {
            if (accounts == null) return null;
            if (accounts.Count == 0) return null;

            StringBuilder buffer = new StringBuilder();
            foreach (string item in accounts)
            {
                if (buffer.Length == 0) buffer.Append(string.Format("'{0}'", item));
                else buffer.Append(string.Format(",'{0}'", item));
            }
            string sql = string.Format("{0} where c.account in ({1})", defaultAccountQuery, buffer);
            LoadAccounts(sql);
            List<AccountInfo> res = new List<AccountInfo>();
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                foreach (string item in accounts)
                {
                    if (innerAccounts.ContainsKey(item))
                        res.Add(innerAccounts[item]);
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
            return res;
        }

        public List<AccountInfo> AccountsTradingLimitGrossOf(List<string> accounts, string sql)
        {
            if (accounts == null) return null;
            if (accounts.Count == 0) return null;

            StringBuilder buffer = new StringBuilder();
            foreach (string item in accounts)
            {
                if (buffer.Length == 0) buffer.Append(string.Format("'{0}'", item));
                else buffer.Append(string.Format(",'{0}'", item));
            }
            string sqlTLG = string.Format("{0} where AccountID in ({1})", sql, buffer);
            LoadAccountsTradingLimitGross(sqlTLG);
			List<AccountInfo> res = new List<AccountInfo>();
            omsCommon.AcquireSyncLock(innerAccounts);
            try
            {
                foreach (string item in accounts)
                {
                    if (innerAccounts.ContainsKey(item))
                        res.Add(innerAccounts[item]);
                }
            }
            finally
            {
                omsCommon.ReleaseSyncLock(innerAccounts);
            }
            return res;
        }
    }
}