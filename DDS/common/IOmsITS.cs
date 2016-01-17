using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common
{
    public interface IOmsITS : IDisposable
    {
        bool Verify();
        void VerifySSM(string cmd);
        void QueryPosition(string query);
        void QueryBODPosition(string query);
        void QueryHistory(string query);
        void QuerySuborder();
        void QueryTrade();
        void QueryAccount(string account);
        void CustomQuery(string query);
        void RedirectOrderCommand(string cmd);
        void RedirectDDSCommand(string cmd);
    }
}
