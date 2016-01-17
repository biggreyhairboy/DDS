using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMS.lib;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;

using OMS.common;
using OMS.common.Sockets;

namespace OMS
{
    class DDSRun
    {
        static void Main(string[] args)
        {
            CServer cserver = new CServer();
            cserver.Start();
            Console.WriteLine("DDS begin listening");
            Console.ReadLine();
        }
    }
}
