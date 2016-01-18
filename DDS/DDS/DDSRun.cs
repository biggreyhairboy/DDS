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
            string DDSIP;
            int DDSPort;
            LoadDDSConfig(out DDSIP, out DDSPort);
            CServer cserver = new CServer(DDSIP, DDSPort);
            cserver.Start();
            Console.WriteLine("DDS begin listening");
            Console.ReadLine();
        }

        static void LoadDDSConfig(out string ip,out int port)
        {
            //加载日志配置
            // ISynchronizeInvoke iv = new ISynchronizeInvoke();
            log4net.Config.XmlConfigurator.Configure();
            omsLog.log.Info(System.Environment.Version.ToString());

            //TODO:  将都配置的部分独立
            NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection("DDSGROUP/OMS_SVR");
            ip = config["ip"];
            port = Convert.ToInt32(config["port"]);
            //J
        }
    }
}
