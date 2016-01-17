using System;

namespace OMS.common.Models.AccountModel
{
    public class AccountInfo
    {
        protected string accountID;
        protected string userID;
        protected string firstName;
        protected string lastName;
        protected string englishName;
        protected string accountName;
        protected string chiName;
        protected string dayPhone;
        protected string homePhone;
        protected string mobile;
        protected string fax;
        protected string email;
        protected string interestAccrualDate;
        protected string shortcutKey;
        protected string clientClass;
        protected string clientTitle;
        protected string accountNature;
        protected string suspendStock;
        protected string address1;
        protected string address2;
        protected string address3;
        protected string officeTel;
        protected string masterAccount;
        protected string password;
        protected string currency;
        protected string entitlement;
        protected string nationalID;
        protected string gender;
        protected string customerType;

        protected bool isSuspend;
        protected bool isNewLimit;
        protected bool dataDynamic;
        protected bool dirty;

        protected int marginType;
        protected int roleType;

        protected decimal bodLotLimit;
        protected decimal usedLotLimit;
        protected decimal bodWebTradeLimit;
        protected decimal currWebTradeLimit;
        protected decimal unclrChequeQty;
        protected decimal interestAccrual;
        protected decimal marginLoadLimit;
        protected decimal bodTradingLimit;
        protected decimal currTradingLimit;
        protected decimal usedTradingLimit;
        protected decimal bodCashBalance;
        protected decimal bodSDCashBalance;
        protected decimal sdCashBalance;
        protected decimal cashBalance;
        protected decimal heldMargin;
        protected decimal maintainMargin;
        protected decimal pnl;
        protected decimal marginCall;
        protected decimal commRate;
        protected decimal minComm;
        protected decimal accountComm;
        protected decimal cashWithdraw;
        protected decimal cashDeposit;
        protected decimal pendingApproveAmt;
        protected decimal forthComeLimit;
        protected decimal withdrawableBalance;
        protected decimal initDTL;
        protected decimal availDTL;
        protected decimal valiableBalance;
        protected decimal tradingLimitGross;
        protected decimal cutLossValue;
        protected decimal requiredMargin;
        protected decimal initialMargin;
        protected decimal holdAmount;
        protected decimal netEquity;

        public AccountInfo(string account)
        {
            this.accountID = account;
            dataDynamic = true;
            dirty = false;
        }

        public string Account { get { return accountID; } }

        public bool Dirty
        {
            get { return dirty; }
            internal set
            {
                if (dirty != value)
                    dirty = value;
            }
        }

        public string EnglishName { get { return englishName; } set { englishName = value; } }

        public string Address1 { get { return address1; } set { address1 = value; } }

        public string Address2 { get { return address2; } set { address2 = value; } }

        public string Address3 { get { return address3; } set { address3 = value; } }

        public string OfficeTel { get { return officeTel; } set { officeTel = value; } }

        public string UserID { get { return userID; } set { userID = value; } }

        public string FirstName { get { return firstName; } set { firstName = value; } }

        public string LastName { get { return lastName; } set { lastName = value; } }

        public string AccountName { get { return accountName; } set { accountName = value; } }

        public string ChineseName { get { return chiName; } set { chiName = value; } }

        public string DayPhone { get { return dayPhone; } set { dayPhone = value; } }

        public string HomePhone { get { return homePhone; } set { homePhone = value; } }

        public string Mobile { get { return mobile; } set { mobile = value; } }

        public string Fax { get { return fax; } set { fax = value; } }

        public string eMail { get { return email; } set { email = value; } }

        public string ClientClass { get { return clientClass; } set { clientClass = value; } }

        public string ClientTitle { get { return clientTitle; } set { clientTitle = value; } }

        public string InterestAccrualDate { get { return interestAccrualDate; } set { interestAccrualDate = value; } }

        public string ShortcutKey { get { return shortcutKey; } set { shortcutKey = value; } }

        public string AccountNature { get { return accountNature; } set { accountNature = value; } }

        public string SuspendStock { get { return suspendStock; } set { suspendStock = value; } }

        public string MasterAccount { get { return masterAccount; } set { masterAccount = value; } }

        public string Password { get { return password; } set { password = value; } }

        public string Currency { get { return currency; } set { currency = value; } }

        public string Entitlement { get { return entitlement; } set { entitlement = value; } }

        public string NationalID { get { return nationalID; } set { nationalID = value; } }

        public string Gender { get { return gender; } set { gender = value; } }

        public string CustomerType { get { return customerType; } set { customerType = value; } }

        public bool IsSuspend { get { return isSuspend; } set { isSuspend = value; } }

        public bool IsNewLimit { get { return isNewLimit; } set { isNewLimit = value; } }

        public bool DataDynamic { get { return dataDynamic; } set { dataDynamic = value; } }

        public int MarginType { get { return marginType; } set { marginType = value; } }

        public int RoleType { get { return roleType; } set { roleType = value; } }

        public decimal BodWebTradingLimit { get { return bodWebTradeLimit; } set { bodWebTradeLimit = value; } }

        public decimal WebTradingLimit { get { return currWebTradeLimit; } set { currWebTradeLimit = value; } }

        public decimal BodTradingLimit { get { return bodTradingLimit; } set { bodTradingLimit = value; } }

        public decimal TradingLimit { get { return currTradingLimit; } set { currTradingLimit = value; } }

        public decimal BodCashBalance { get { return bodCashBalance; } set { bodCashBalance = value; } }

        public decimal CashBalance { get { return cashBalance; } set { cashBalance = value; } }

        public decimal PendingApproveAmt { get { return pendingApproveAmt; } set { pendingApproveAmt = value; } }

        public decimal UnclrChequeQty { get { return unclrChequeQty; } set { unclrChequeQty = value; } }

        public decimal InterestAccrual { get { return interestAccrual; } set { interestAccrual = value; } }

        public decimal MaringLoadLimit { get { return marginLoadLimit; } set { marginLoadLimit = value; } }

        public decimal BodSDCashBalance { get { return bodSDCashBalance; } set { bodSDCashBalance = value; } }

        public decimal SDCashBalance { get { return sdCashBalance; } set { sdCashBalance = value; } }

        public decimal HeldMargin { get { return heldMargin; } set { heldMargin = value; } }

        public decimal MaintainMargin { get { return maintainMargin; } set { maintainMargin = value; } }

        public decimal PnL { get { return pnl; } set { pnl = value; } }

        public decimal MarginCall { get { return marginCall; } set { marginCall = value; } }

        public decimal CommRate { get { return commRate; } set { commRate = value; } }

        public decimal MinComm { get { return minComm; } set { minComm = value; } }

        public decimal AccountComm { get { return accountComm; } set { accountComm = value; } }

        public decimal CashWithdraw { get { return cashWithdraw; } set { cashWithdraw = value; } }

        public decimal CashDeposit { get { return cashDeposit; } set { cashDeposit = value; } }

        public decimal BodLotLimit { get { return bodLotLimit; } set { bodLotLimit = value; } }

        public decimal UsedLotLimit { get { return usedLotLimit; } set { usedLotLimit = value; } }

        public decimal ForthComeLimit { get { return forthComeLimit; } set { forthComeLimit = value; } }

        public decimal WithdrawableBalance { get { return withdrawableBalance; } set { withdrawableBalance = value; } }

        public decimal InitialDailyTradingLimit { get { return initDTL; } set { initDTL = value; } }

        public decimal AvailableDailyTradingLimit { get { return availDTL; } set { availDTL = value; } }

        public decimal ValiableBalance { get { return valiableBalance; } set { valiableBalance = value; } }

        public decimal TradingLimitGross { get { return tradingLimitGross; } set { tradingLimitGross = value; } }

        public decimal CutLossValue { get { return cutLossValue; } set { cutLossValue = value; } }

        public decimal RequiredMargin { get { return requiredMargin; } set { requiredMargin = value; } }

        public decimal InitialMargin { get { return initialMargin; } set { initialMargin = value; } }

        public decimal HoldAmount { get { return holdAmount; } set { holdAmount = value; } }

        public decimal NetEquity { get { return netEquity; } set { netEquity = value; } }
    }
}