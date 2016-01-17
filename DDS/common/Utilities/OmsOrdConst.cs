using System;

namespace OMS.common.Utilities
{
    public class OmsOrdConst
    {
        public const int omsOrderMarket = 1;
        public const int omsOrderNotHeld = 2;
        public const int omsOrderOddLot = 3;
        public const int omsOrderStop = 4;
        public const int omsOrderVWAP = 5;
        public const int omsOrderTrade = 6;
        public const int omsOrderSynthetic = 7;
        public const int omsOrderEnum = 31;
        public const int omsOrderManual = 32;
        public const int omsOrderNocheck = 64;
        public const int omsOrderFAK = 128;
        public const int omsOrderFOK = 256;
        public const int omsOrderPreOpen = 512;
        public const int omsOrderEnhancedLimit = 1024;
        public const int omsOrderStopLimit = 2048;
        public const int omsOrderReverse = 4096;
        public const int omsOrderNoComm = 8192;
        public const int omsOrderUseSSell = 16384;
        public const int omsOrderSettle = 32768;
        public const int omsOrderBasket = 65536;
        public const int omsOrderDirect = 131072;
        public const int omsOrderBook = 262144;
        public const int omsOrderAddTrade = 524288;
        public const int omsOrderGTC = 1048576;
        public const int omsOrderAutoFill = 2097152;
        public const int omsOrderAtsInactive = 4194304;
        public const int omsOrderTriggerUp = 8388608;
        public const int omsOrderTriggerDown = 16777216;
        public const int omsOrderCombo = 33554432;
        public const int omsOrderFixPrice = 67108864;
        public const int omsOrderReqApproval = 134217728;
        public const int omsOrderRejApproval = 268435456;
        public const int omsOrderAccApproval = 536870912;
        public const int omsOrderNotNull = 1 << 31;//1 shl 31;
        public const int omsBasketNonIndex = 131072;
        public const int omsBasketArbitrage = 262144;
        public const int omsBasketIndex = 524288;
        public const int omsBasketPortInsu = 1048576;
        public const int omsBasketAssetAllocat = 2097152;
        public const int omsBasketHedg = 4194304;
        public const int omsBasketOther = 8388608;

        public const int omsOrder1Work = 1;
        public const int omsOrder1Away = 2;
        public const int omsOrder1StoreTillMarket = 4;
        public const int omsOrder1PreTrdAllocation = 8;
        public const int omsOrder1UnClearCheck = 16;
        public const int omsOrder1inPortfolio = 32;
        public const int omsOrder1Confirmed = 64;
        public const int omsOrder1DupMessage = 128;
        public const int omsOrder1Void = 256;
        public const int omsOrder1SkipAOChk = 512;
        public const int omsREQ1 = 1024;
        public const int omsREQ1Release = 2048;
        public const int omsREQ1Abort = 4096;
        public const int omsOrder1Group = 8192;
        public const int omsOrder1CD = 16384;
        public const int omsOrder1Bulk = 32768;
        public const int omsOrder1MultiBasket = 65536;
        public const int omsOrder1PercentBasket = 131072;
        public const int omsOrder1Alert = 262144;
        public const int omsOrder1MOLimit = 524288;
        public const int omsOrder1MOStop = 1048576;
        public const int omsOrder1TLVoucher = 2097152;
        public const int omsOrder1TrdMv = 4194304;
        public const int omsOrder1OpenEndWork = 8388608;
        public const int omsOrder1MarginLoaded = 16777216;
        public const int omsOrder1Approval = 33554432;
        public const int omsOrder1SellRound = 67108864;
        public const int omsorder1AlgoManaged = 1 << 27;//1 shl 27;
        public const int omsOrder1WorkParent = 1 << 28;//1 shl 28;
        public const int omsOrder1WorkChild = 1 << 29;//1 shl 29;
        public const int omsOrder1ManualChangeWorkParent = 1073741824;

        public const int omsOrder2SI_CASH = 1;
        public const int omsOrder2SI_SECURITY = 2;
        public const int omsOrder2ComplianceCheck = 256;
        public const int omsOrder2LateTrade = 262144;
        public const int omsOrder2TradeAmendment = 524288;

        public const int omsOrder3DisplayOnly = 1;
        public const int omsOrder3ApprovalWarning = 2;
        public const int omsOrder3Dest = 4;
        public const int omsOrder3FundInstruct = 8;
        public const int omsOrder3CombineParent = 16;
        public const int omsOrder3CombineChild = 32;
        public const int omsOrder3ManualCancel = 64;
        public const int omsOrder3InLock = 128;

        public const int omsOrder4SmartQuantityChange = 256;
        public const int omsOrder4ForceAddInactiveOrder = 512;
        public const int omsOrder4NoWorkAllow = 1024;
        public const int omsOrder4SkipOddLot = 2048;
        public const int omsOrder4RejectOnExchError = 4096;
        public const int omsOrder4NonExecutable = 8192;
        public const int omsOrder4NonManualUnmanaged = 16384; // Louis: With this flag set, when order goes into unmanaged mode, it will not set manual instruct as well.
        public const int omsOrder4SkipPriceCheck = 32768;
        public const int omsOrder4SkipQtyCheck = 65536;
        public const int omsOrder4SmartShortSell = 131072; // Louis: The portion of insufficient quantity will be sent with shortsell flag automatically
        public const int omsOrder4ForceCreditConsume = 262144; // Louis: Indicating the credit will be checked and deducted regardless of order type 
        public const int omsOrder4DayEndCancel = 524288; // Louis: If set, all the outstanding exchange order will be treated as cancelled after day close.
        public const int omsOrder4HKMEXTypeChange = 1048576; // set this flag to identify this order is HKMEx special change order type.
        public const int omsOrder4OAMutualFund = 2097152;

        public const int omsOrderStatusBooked = 1;
        public const int omsOrderStatusAllocated = 2;
        public const int omsOrderStatusUnmanaged = 4;
        public const int omsOrderStatusDoneForTheDay = 8;

        public const int omsOrderExchStatusInternalTrigger = 256; // Louis: Indicated that the last exch order request was triggered by order management control, so order server could identify unexpected exchange error.
        public const int omsOrderExchStatusFollowUp = 512; // Louis: Indicated that this exch order should be followed up in order handling (price chasing for position exposure part) 

        public const int omsChangeInstruct = -1;
        public const int omsChangeInstruct1 = omsOrder1Alert;
        public const int omsChangeInstruct2 = 0;
        public const int omsChangeInstruct3 = 0;
        //public const int omsChangeInstruct4 = 0;
        public const int omsChangeInstruct4 = omsOrder4SkipPriceCheck + omsOrder4SkipQtyCheck;

        public const int omsErrorOddLot = 1;
        public const int omsErrorOK = 2;
        public const int omsErrorNotReady = 3;
        public const int omsErrorNotHandle = 4;
        public const int omsErrorAccount = -1;
        public const int omsErrorOrderType = -2;
        public const int omsErrorNeedSymbol = -3;
        public const int omsErrorNeedQuantity = -4;
        public const int omsErrorInsufStock = -5;
        public const int omsErrorInsufFund = -6;
        public const int omsErrorInvalidTick = -7;
        public const int omsErrorPriceRange = -8;
        public const int omsErrorQuantity = -9;
        public const int omsErrorDatabase = -10;
        public const int omsErrorClntValidate = -11;
        public const int omsErrorNotOutstanding = -12;
        public const int omsErrorInvalidExchange = -14;
        public const int omsErrorDisapproved = -15;
        public const int omsErrorExchange = -16;
        public const int omsErrorStopPrice = -17;
        public const int omsErrorOverTranLimit = -18;
        public const int omsErrorSpanInfoNotFound = -36;

        public const int omsMinErrorOverSliceLmt = -180002;
        public const int omsMinErrorOverTotalLmt = -180003;
        public const int omsMinErrorAcctDailyLmt = -180004;

        public const int omsErrorSymbol = -19;
        public const int omsErrorLotSizeZero = -20;
        public const int omsErrorICL = -21;
        public const int omsErrorMktClose = -22;
        public const int omsErrorChangeAO = -23;
        public const int omsErrorApproval = -24;
        public const int omsErrorUnderQuantity = -25;
        public const int omsErrorInactiveLimit = -26;
        public const int omsErrorHoldFundSystem = -27;
        public const int omsMinErrorInsufFund = -60002;
        public const int omsMinErrorSystemFail = -270002;
        public const int omsMinErrorReleaseFund = -270003;
        public const int omsMinErrorChgRelease = -270004;
        public const int omsMinErrorChgHold = -270005;
        public const int omsErrorAmend = -28;
        public const int omsMinErrorAmendUp = -280001;
        public const int omsAmendBlocked = -29;
        public const int omsMinErrorAmendBlocked = -290001;
        public const int omsErrorNeedConfirm = -30;
        public const int omsErrorChangeExchOrder = -31;
        public const int omsErrorCancelExchOrder = -32;
        public const int omsErrorRejectAO = -33;
        public const int omsMinErrorSEHKRejAO = -330001;
        public const int omsMinErrorHKFEAOBuy = -330002;
        public const int omsMinErrorHKFEAOSell = -330003;
        public const int omsErrorMarginCall = -34;
        public const int omsMinErrorStockMarginCall = -340001;
        public const int omsErrorWorkOrder = -35;
        public const int omsMinErrorNotComplete = -350001;

        public const int omsProductStock = 1;
        public const int omsProductFuture = 2;
        public const int omsProductOption = 3;
        public const int omsProductWarrant = 4;
        public const int omsProductBasketWarrant = 5;
        public const int omsProductBond = 6;
        public const int omsProductTrust = 7;
        public const int omsProductCurrency = 8;
        public const int omsProductStockIndex = 9;
        public const int omsProductFund = 10;

        public const int omsOrderBuy = 0;
        public const int omsOrderSell = 1;

        public const int omsOrderOpen = 1;
        public const int omsOrderClose = 2;

        public const int omsOrderTagForceHoldFund = 10;

        public const int omsProfileLevel = 1;
        public const int omsBrokerLevel = 2;
        public const int omsBasicLevel = 4;
        public const int omsExchangeLevel = 8;
        public const int omsCreditLevel = 16;
        public const int omsPositionLevel = 32;
        public const int omsHoldFundLevel = 64;
        public const int omsReCheckLevel = 128;
        public const int omsDepositSecurityLevel = 256;
        public const int omsDepositFundLevel = 512;
        public const int omsForceCreditLevel = 1024;
        public const int omsForcePositionLevel = 2048;
        public const int omsExchSendLevel = 4096;
        public const int omsSkipCheckLevel = 8192;

        public const int omsSaveOrderLevel = 16384;
        public const int omsRecoverLevel = 32768;
        public const int omsDTLLevel = 65536;

        public const int amsOrderNewTradesCalc = 1;
    }
}
