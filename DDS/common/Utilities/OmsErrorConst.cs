using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.Utilities
{
    public class OmsErrorConst
    {
        public const int OMS_PERMDENIED = -1;
        public const int OMS_NOAVAILABLE = -2;
        public const int OMS_SESSIONFAILED = -3;

        // Error Code for SSM
        public const int OMS_SSMERR_NOERROR = 0;   // No Error
        public const int OMS_SSMERR_LOGINFAILED = 10;  // login failed
        public const int OMS_SSMERR_PWDCHGFAILED = 11;  // password change failed
        public const int OMS_SSMERR_PWDEXPIRED = 12;  // password expired
        public const int OMS_SSMERR_USERSUSPENDED = 13;  // user suspended/disabled
        public const int OMS_SSMERR_PWDCHECKFAILED = 14;  // password checking failed
        public const int OMS_SSMERR_ACCTEXPIRED = 15;
        public const int OMS_SSMERR_VERIFYFAILED = 20;  // verify failed
        public const int OMS_SSMERR_VERIFYTIMEOUT = 30;  // remote verify timeout
        public const int OMS_SSMERR_LOGOUTFAILED = 40;  // logout failed
        public const int OMS_SSMERR_OPERDENIED = 41;  // operation denied
        public const int OMS_SSMERR_NEWSESSION = 50;  // Session Terminated due to new session
        public const int OMS_SSMERR_SESSIONEXPIRE = 51;  // Session Expired
        public const int OMS_SSMERR_SESSIONKICK = 52;  // Session is kicked
        public const int OMS_SSMERR_SESSIONLOGOUT = 53;  // User logout normally
        public const int OMS_SSMERR_VERIFYPROTECT = 54;  // Session Terminated due to verify retry failed
        public const int OMS_SSMERR_NOCERTIFICATE = 60;     // The License file does not contain this certificate
        public const int OMS_SSMERR_NOTENOUGHTLICENSE = 61; // The application excesses license limit
        public const int OMS_SSMERR_NOAPPLICATIONID = 62;   // No application ID in login command
        public const int OMS_SSMERR_NOTENOUGHTUSER = 63;
        public const int OMS_SSMERR_NOTENOUGHTACCT = 64;
        public const int OMS_SSMERR_NOTAVAL = 65;           // The counterpart ssm is not avaliable for any reason
        public const int OMS_SSMERR_PRISERVAVAL = 66;       // The counterpart ssm exist
        public const int OMS_SSMERR_WAITPRISER = 67;        // The ssm is waiting counterpart's response
        public const int OMS_SSMERR_BACKSEREXPIRE = 68;     // The backup ssm is expired
        public const int OMS_SSMERR_NOCONNECTION = -999;//no connection

        // Error code for iTraderServer (ITS)
        public const int OMS_ITSERR_COMMUNKNOWN = 110; // command unknown
        public const int OMS_ITSERR_ITSCOMMINVALID = 111; // ITS command invalid
        public const int OMS_ITSERR_VERIFYREQ = 112; // verification required
        public const int OMS_ITSERR_CIPHERREQ = 113; // Cipher key not set
        public const int OMS_ITSERR_DUPVERIFY = 114; // Verification duplicated
        public const int OMS_ITSERR_DEVICEDENIED = 115; // Device account restricted
        public const int OMS_ITSERR_ADDRESDENIED = 116; // Address access denied
        public const int OMS_ITSERR_ACCTINVALID = 120; // Account ID invalid
        public const int OMS_ITSERR_DAYNOINVALID = 121; // Invalid number of while querying history
        public const int OMS_ITSERR_KEYINVALID = 122; // Cipher key invalid
        public const int OMS_ITSERR_TRADEPWDFAILED = 123; // Trading password verification failed
        public const int OMS_ITSERR_RECNOTFOUND = 130; // Required record not found
        public const int OMS_ITSERR_SETTLEFAILED = 131; // Settle instruction failed
        public const int OMS_ITSERR_EXCHANGECLOSED = 132; // Market closed. Operation rejected.
        public const int OMS_ITSERR_QUERYNOTDEFINED = 133; // Requested Custom Query is not defined
        public const int OMS_ITSERR_QUERYERROR = 134; // Query executing error

        // Error code for Futures Synthesizer
        public const int OMS_FSSERR_COMMUNKNOWN = 110; // command unknown
    }
}