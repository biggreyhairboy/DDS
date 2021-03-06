using System;

namespace OMS.common.Utilities
{
	public class EntitlementConst
	{
		public const string ENT_OrderApprove      = "ORDAppr";
		public const string ENT_OrderApproveBal   = "ORDApprBal";
		public const string ENT_OrderApprovePos   = "ORDApprPos";
		public const string ENT_OrderApprInstance = "ORDApprIn";
		public const string ENT_OrderReject       = "ORDReject";
		public const string ENT_OrderReset        = "ORDReset";
		public const string ENT_OrderInactive     = "ORDInac";
		public const string ENT_OrderInactiveAll  = "ORDInacAll";
		public const string ENT_OrderActive       = "ORDActive";
		public const string ENT_OrderSplit        = "ORDSplit";
		public const string ENT_OrderAdjAccount   = "ORDAdjAcc";
		public const string ENT_OrderChange       = "ORDChange";
		public const string ENT_OrderCancel       = "ORDCancel";
		public const string ENT_OrderCancelAll    = "ORDCancelAll";
		public const string ENT_OrderPass         = "ORDPass";
		public const string ENT_OrderConfirm      = "ORDConfirm";
		public const string ENT_OrderBulkCanc     = "ORDBlxCanc";
		public const string ENT_OrderVoid         = "ORDVoid";
		public const string ENT_OrderApprReq      = "ORDApprReq";
		public const string ENT_OrderStore        = "ORDStore";
		public const string ENT_OrderMarketApp    = "ORDMrkApp";
		public const string ENT_OrderMarketRej    = "ORDMrkRej";
		public const string ENT_OrderPendReject   = "ORDPendRej";
		public const string ENT_OrderWorking      = "ORDWorking";
		public const string ENT_OrderPreAdjAccount = "ORDPreAdjAcc";
		public const string ENT_MKTAMS            = "AMSPrice";
		public const string ENT_MKTMulti          = "MultiPrice";
		public const string ENT_MKTHKFE           = "HKFEPrice";
		public const string ENT_MKTNews           = "NewsFeed";
		public const string ENT_MKTPage           = "SEHKPage";
		public const string ENT_MKTTOP            = "MKTTOP";
		public const string ENT_OrderOddTrade     = "ORDOddTr";
		public const string ENT_OrderManuTrade    = "ORDManuTr";
		public const string ENT_OrderCrossBr      = "ORDCrossBr";
		public const string ENT_OrderMTRoute      = "ORDMTRoute";
		public const string ENT_MarginSim		    = "MarginSim";
		public const string ENT_ExchCancel        = "ExchCancel";
		public const string ENT_QuickPad          = "QuickPad";
		public const string ENT_CHASEP            = "ORDChangeSP";
		public const string ENT_OEHKFE            = "HKFETrade";
		public const string ENT_OESEHK            = "SEHKTrade";
		public const string ENT_OETRADE		    = "%sTrade";
		public const string ENT_OEStopLimit       = "OEStopL";
		public const string ENT_OEOpenClose       = "OEOpenC";
		public const string ENT_OEPrincipal       = "OEPrin";
		public const string ENT_OEAO              = "OEAO";
		public const string ENT_OEFOK             = "OEFOK";
		public const string ENT_OEFAK             = "OEFAK";
		public const string ENT_OEOddlot          = "OEOddLot";
		public const string ENT_OEShortSell       = "OEShortS";
		public const string ENT_OEEnhanced        = "OEEnhanced";
		public const string ENT_OEGoodtillWeek    = "OEGoodtillWeek";
		public const string ENT_OEGoodtillCancel  = "OEGoodtillCancel";
		public const string ENT_OEGoodtillDate    = "OEGoodtillDate";
		public const string ENT_OEMarketorder     = "OEMarketOrder";
		public const string ENT_OEKGIOddLot       = "OEKGIOddLot";
		public const string ENT_OEFileImport      = "OEFileImport";
		public const string ENT_OECustomInfo      = "OECustomInfo";
		public const string ENT_OEOrderRoute      = "OERoute";
		public const string ENT_PL		        = "PLMon";
		public const string ENT_TLMon		        = "TLMon";
		public const string ENT_CTD               = "CTD";
		public const string ENT_CTF               = "CTF";
		public const string ENT_QuoteRequest      = "QRequest";
		public const string ENT_QuoteRefresh      = "MAKQRef";
		public const string ENT_FIRENOSPANCHK     = "FIRENOCHK";
		public const string ENT_FIREADMIN         = "FIREADMIN";
		public const string ENT_FIRE_I_ORDERBOOK  = "FIRE_ORDER";
		public const string ENT_FIRE_I_TRADEBOOK  = "FIRE_TRADE";
		public const string ENT_FIRE_I_USER_TRADESTAT = "FIRE_USTAT";
		public const string ENT_FIRE_RULEORDERENTRY   = "FIRE_RULE";
		public const string ENT_FIRE_JOYSTICK         = "FIRE_JOY";
		public const string ENT_MFTS              = "MFTS";
		public const string ENT_MFTSOPERATION     = "MFTSOperation";
		public const string ENT_CCMICL            = "CCM.ICL";
		public const string ENT_CCMITL            = "CCM.ITL";
		public const string ENT_CCMICB            = "CCM.ICB";
		public const string ENT_CCMILL            = "CCM.ILL";
		public const string ENT_CCMFCL            = "CCM.FCL";
		public const string ENT_SERMANCLIENTLITE  = "SCLITE";
		public const string ENT_PTA               = "PostTradeAllocation";
		public const string ENT_PTACFM            = "PostTradeAllocCfm";
		public const string ENT_AlgoTab           = "AlgoTabOrderEntry";
		public const string ENT_AddSmartQtyOrder = "AddOrderSmartQtyChg";
		public const string ENT_WorkSmartQtyOrder = "WorkOrderSmartQtyChg";
		public const string ENT_OrderDepthRequest = "MAKDepReq";
		public const string ENT_REPORT_DAILYCHARGE  ="REPORD_DC";
		public const string ENT_CUM="ChangeUserMsg";
		public const string ENT_ACK="Acknowledge";
		public const string ENT_AmendmentHistory="AmendmentHistory";
		public const string ENT_OrderREV="QuickPadREV";
		public const string ENT_Reload="DBReload";
		public const string ENT_DCPL="iDoubleClickP&L";
		public const string ENT_OpenEndWork="OpenEndWork";
		public const string ENT_Sales="Sales";
		public const string ENT_ARSO="AutoRoundSellOddLot";
		public const string ENT_DCPLCOST="PnLvsCost";
		public const string ENT_ARBO="AutoRoundBuyOddLot";
		public const string ENT_BOOK="ORDBook";
		public const string ENT_GROUPENTRY="GroupOrderEntry";
		public const string ENT_ENHANCEORDERDEPTH="EnhanceOrderDepth";
		public const string ENT_OEBookOddLot      = "OEBookOddLot";
		public const string ENT_SA_EDIT_DEVICEACCOUNT = "SA.E.A.D";
		public const string ENT_OrdHistory = "OrdHistory";
		public const string ENT_PendOrdCmd = "PendOrdCmd";
		public const string ENT_FindClntByPos = "FindClntByPos";
		public const string ENT_AFEChannel = "AFE";
		public const string ENT_AASTOCKS          = "AASTOCKS";
		public const string ENT_AOS   ="AOS";
		public const string ENT_GENERICORDER  = "GO";
		public const string ENT_ORDERENTRY = "OrderEntry";
		public const string ENT_FLOATINGENTRY = "FloatingOrderEntry";
		public const string ENT_FUTUREENTRY = "FutureOrderEntry";
		public const string ENT_TRADEMOVE="TRADEMOVE";
		public const string ENT_MARGINCALLWINDOW="MARGINCALLWINDOW";
		public const string ENT_BestPrice = "ORDBestPrice";
		public const string ENT_PROChart = "ProChart";
		public const string ENT_IntraD_Week = "IntraD_Week";
		public const string ENT_IntraD_Day = "IntraD_Day";
		public const string ENT_IntraD_1 = "IntraD_1";
		public const string ENT_IntraD_3 = "IntraD_3";
		public const string ENT_IntraD_5 = "IntraD_5";
		public const string ENT_TickByTick = "TickByTick";
		public const string ENT_Realtime = "Realtime";
		public const string FID_OMSCLIENT = "OMSClient";
		public const string FID_FIRECLIENT = "FIREClient";
        public const string ENT_RealtimeMonitor = "RealtimeMonitor";
        public const string ENT_RTMonDayEndReport = "RTMonDayEndReport";
        public const string ENT_MASTERUSER = "MasterUser";
        public const string ENT_TEWEB = "TEWeb";
        public const string ENT_SKIPTRADINGPASSWORD = "SKIPTRADINGPASSWORD";
        public const string ENT_FMSTATION = "FMStation";
        public const string ENT_FMS_Portfolio = "FMS.Portfolio";
        public const string ENT_FMS_CashAlloc = "FMS.CashAlloc";
        public const string ENT_FMS_Benchmark = "FMS.Benchmark";
        public const string ENT_FMS_RiskAnalysis = "FMS.RiskAnalysis";
        public const string ENT_FMS_MultiFactor = "FMS.MultiFactor";
        public const string ENT_FMS_SimpleEntry = "FMS.SimpleEntry";
        public const string ENT_FMS_Rebalance = "FMS.Rebalance";
        public const string ENT_FMS_OrderBook = "FMS.OrderBook";
        public const string ENT_FMS_MF_SystemLevel = "FMS.MF.SystemLevel";
        public const string ENT_FMS_MF_UserLevel = "FMS.MF.UserLevel";
	}
}
