using System;

namespace OMS.common.Utilities
{
    public enum OmsDataState
    {
        dsNew = 0,
        dsUpdated = 1
    }

    public enum OmsCallPut
    {
        cpUndefined = 0,
        cpCall = 1,
        cpPut = 2
    }

    public enum OmsAvgPriceMode
    {
        /// <summary>
        /// First in first net
        /// </summary>
        apmFIFN = 0,
        /// <summary>
        /// A Share
        /// </summary>
        apmASHARE
    }

    public enum OmsSQLError
    {
        seConnectionFailed = -1,
        seOK = 0,
        seGenericFailed = 1
    }

    public enum DDSMode
    {
        dmImage = 0,
        dmUpdate,
        dmBoth
    }

    public enum UpdateType
    {
        utNone = 0,
        utPrice,
        utOrder,
        utSuborder,
        utTrade
    }

    public enum SSMSessionState
    {
        ssKillSession = -2,
        ssLogout = -1,
        ssTimeout = 0,
        ssValid = 1,
        ssDuplicateLogin = 2
    }
}
