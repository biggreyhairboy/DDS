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
    class CServer: TSocketServer 
    {
        public CServer(string ip, int port):base(ip, port)
        {
            DDSInitializer();
        }

        private void DDSInitializer()
        {
            this.OnClientConnect += DDSServer_OnClientConnect;
            this.OnClientDisconnect += DDSServer_OnClientDisconnect;
            this.OnClientMessage += DDSServer_OnClientMessage;
           // this.Start();
        }

        private void DDSServer_OnClientDisconnect(object sender, SocketClientConnectEventArgs e)
        {
            omsLog.log.Debug("client disconnectted!");
        }

        private void DDSServer_OnClientMessage(object sender, SocketServerReceiveEventArgs e)
        {

            Console.WriteLine("Incomming messsage: " + e.Message);
           // DDSServer.SendClientMsg("writeback: i have received you request!")
            
            //todo: 至少需要保持两个dict 1. 客户端根据名称来索引  2.信息根据各种类型特征来索引
        }

        private void DDSServer_OnClientConnect(object sender, SocketClientConnectEventArgs e)
        {
            omsLog.log.Debug("client connectted!");
            //((TSocketClient)sender).
            //client
            clientList[e.ClientID].SendMessage("call you back later");
        }
    }
}
