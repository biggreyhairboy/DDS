using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Configuration;
using System.Collections.Specialized;

using OMS.lib;
using OMS.common.Sockets;

namespace OMS
{
    class CServer
    {
        public TSocketServer DDSServer;
        public string DDSIP;
        public int DDSPort;

        public CServer()
        {
            LoadDDSConfig();
            DDSInitializer();
        }

        private void DDSInitializer()
        {
            DDSServer = new TSocketServer(DDSIP, DDSPort, TSocketDataMode.sdmOmsLine);
            DDSServer.OnClientConnect += DDSServer_OnClientConnect;
            DDSServer.OnClientDisconnect += DDSServer_OnClientDisconnect;
            DDSServer.OnClientMessage += DDSServer_OnClientMessage;

        }

        public void Start()
        {
            DDSServer.Start();
        }

        private void LoadDDSConfig()
        {
            //加载日志配置
            // ISynchronizeInvoke iv = new ISynchronizeInvoke();
            log4net.Config.XmlConfigurator.Configure();
            omsLog.log.Info(System.Environment.Version.ToString());

            //todo: 将都配置的部分独立
            NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection("DDSGROUP/OMS_SVR");
            DDSIP = config["ip"];
            DDSPort = Convert.ToInt32(config["port"]);
            //J
        }

        private void DDSServer_OnClientDisconnect(object sender, SocketClientConnectEventArgs e)
        {
            omsLog.log.Debug("client disconnectted!");
        }

        private void DDSServer_OnClientMessage(object sender, SocketServerReceiveEventArgs e)
        {

            Console.WriteLine("Incomming messsage: " + e.Message);

            //todo: 至少需要保持两个dict 1. 客户端根据名称来索引  2.信息根据各种类型特征来索引
        }

        private void DDSServer_OnClientConnect(object sender, SocketClientConnectEventArgs e)
        {
            omsLog.log.Debug("client connectted!");
        }
    }
}
