using System;
using System.Collections.Generic;

namespace OMS.common.Models.AccountModel
{
    public class UserInfo
    {
        protected string userID;
        protected string userName;
        protected string dummyAccount;
        protected string userGroup;
        protected string userPassword;
        protected string passwordHash;
        protected string mail;

        protected int status;
        protected int adminType;
        protected int timeoutDuration;

        protected decimal tradeLimit;

        protected List<string> members;

        public UserInfo(string userID)
        {
            this.userID = userID;
            members = new List<string>();
        }

        public string UserID { get { return userID; } }

        public string UserName { get { return userName; } set { userName = value; } }

        public string DummyAccount { get { return dummyAccount; } set { dummyAccount = value; } }

        public string UserGroup { get { return userGroup; } set { userGroup = value; } }

        public string UserPassword { get { return userPassword; } set { userPassword = value; } }

        public string PasswordHash { get { return passwordHash; } set { passwordHash = value; } }

        public int Status { get { return status; } set { status = value; } }

        public int AdminType { get { return adminType; } set { adminType = value; } }

        public int Timeout { get { return timeoutDuration; } set { timeoutDuration = value; } }

        public decimal TradeLimit { get { return tradeLimit; } set { tradeLimit = value; } }

        public string eMail { get { return mail; } set { mail = value; } }

        public List<string> Members { get { return members; } }
    }
}