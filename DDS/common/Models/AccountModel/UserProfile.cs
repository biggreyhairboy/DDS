using System;
using System.Data;
using OMS.common.Utilities;
using OMS.common.Database;
using System.Collections.Generic;

namespace OMS.common.Models.AccountModel
{
    public class UserProfile
    {
        private static UserProfile instance;

        protected UserInfo user;
        protected List<string> entitlements;
        protected string databaseAlias;
        protected string sessionID;
        /// <summary>
        /// User profile
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <param name="databaseAlias">Database alias for loading user information from the specified database</param>
        public UserProfile(string userID, string databaseAlias)
        {
            user = new UserInfo(userID);
            entitlements = new List<string>();
            sessionID = "";
            this.databaseAlias = databaseAlias;
            LoadUser();
        }

        public UserInfo User { get { return user; } }
        /// <summary>
        /// Gets or sets user's entitlements for further usage
        /// </summary>
        public List<string> Entitlements { get { return entitlements; } set { entitlements = value; } }
        /// <summary>
        /// Gets or sets session ID for SSM login/logout
        /// </summary>
        public string SessionID { get { return sessionID; } set { sessionID = value; } }

        private void LoadUser()
        {
            try
            {
                DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, string.Format("select * from User_File where User_No = '{0}'", user.UserID));

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    user.AdminType = OmsHelper.GetIntFromRow(row, "AdminUser");
                    user.DummyAccount = OmsHelper.GetStringFromRow(row, "DummyAc");
                    string str = OmsHelper.GetStringFromRow(row, "Member");
                    if (str != null && str.Trim() != "")
                    {
                        string[] pieces = str.Split(new char[] { '\r', '\n' });
                        foreach (string item in pieces)
                        {
                            if (item == null || item.Trim() == "") continue;
                            user.Members.Add(item.Trim());
                        }
                    }
                    user.PasswordHash = OmsHelper.GetStringFromRow(row, "PasswordHash");
                    user.Status = OmsHelper.GetIntFromRow(row, "Active");
                    user.Timeout = OmsHelper.GetIntFromRow(row, "TimeoutDuration");
                    user.TradeLimit = OmsHelper.GetDecimalFromRow(row, "TradeLimit");
                    user.UserGroup = OmsHelper.GetStringFromRow(row, "User_Group");
                    user.UserName = OmsHelper.GetStringFromRow(row, "Full_Name");
                    user.UserPassword = OmsHelper.GetStringFromRow(row, "User_Password");
                    user.eMail = OmsHelper.GetStringFromRow(row, "E_mail");
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public static UserProfile Instance { get { return instance; } }

        public static void SetInstance(UserProfile item)
        {
            instance = item;
        }
    }
}