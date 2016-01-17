using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.ComponentModel;

namespace OMS.common
{
    public sealed class omsCommon
    {
        /// <summary>
        /// Basic currency of OMS, default is HKD
        /// </summary>
        public static string BasicCurrency = "HKD";
        /// <summary>
        /// Flag for Option Profit Loss Price market checking, default FALSE
        /// </summary>
        public static bool OptionPLPriceCheckMarket = false;
        /// <summary>
        /// Flag for indicating whether or not log down price information in log file, default TRUE
        /// </summary>
        public static bool LogPrice = true;
        ///<summary>
        /// Flag for indicating wherther or not log down debug information,default is false
        /// </summary>
        public static bool LogDebugInfo = false;
        /// <summary>
        /// Update interval for symbol price change, in second, default 5 seconds
        /// </summary>
        public static int PriceTriggerInterval = 5;
        /// <summary>
        /// Mode for average price calculation, default OmsAvgPriceMode.apmFIFN
        /// </summary>
        public static OmsAvgPriceMode AveragePriceMode = OmsAvgPriceMode.apmFIFN;
        /// <summary>
        /// Mode for P&L calculation, default OmsAvgPriceMode.apmFIFN
        /// </summary>
        public static OmsAvgPriceMode PAndLMode = OmsAvgPriceMode.apmFIFN;
        /// <summary>
        /// Global sync invoker for single threading model, if null value detected, means multi-threading's running
        /// </summary>
        public static ISynchronizeInvoke SyncInvoker;
        ///<summary>
        /// Indicating whether or not the sync invoker has been disposed, default FALSE
        /// </summary>
        public static bool IsSyncInvokerDisposed = false;
        /// <summary>
        /// Flag for indicating whether select margin ratio or not
        ///</summary>
        public static bool NeedMarginRatio = false;
        /// <summary>
        /// If > 0, then try to reconnect the specified times;
        /// Else no limit to the reconnect times(Default value).
        /// </summary>
        public static int MaxReconnectTimes = -1;
        /// <summary>
        /// Acquire synchonize lock for object <paramref name="item"/>
        /// </summary>
        /// <param name="item">Sync lock item</param>
        /// <remarks>
        /// Once AcquireSyncLock function is called, ReleaseSyncLock must be called, too
        /// </remarks>
        public static void AcquireSyncLock(object item)
        {
            if (item == null) return;
            if (SyncInvoker == null)
                System.Threading.Monitor.Enter(item);
        }
        /// <summary>
        /// Release the synchonize lock for object <paramref name="item"/>
        /// </summary>
        /// <param name="item">Sync lock item</param>
        public static void ReleaseSyncLock(object item)
        {
            if (item == null) return;
            if (SyncInvoker == null)
                System.Threading.Monitor.Exit(item);
        }
    }
}
