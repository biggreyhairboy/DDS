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
    class DDSServer: TSocketServer 
    {
        public string serviceName;
        public TSocketClient currentClient;
        public string currentMsg;
        public List<string> openall;

        public Dictionary<string, string> TSubTable;
        public string RcvMessage;


        //public Dictionary<string, oms>

        public DDSServer(string ip, int port):base(ip, port)
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
            clientList[e.ClientID].Disconnect();
            clientList.Remove(e.ClientID);
        }

        private void DDSServer_OnClientMessage(object sender, SocketServerReceiveEventArgs e)
        {

            Console.WriteLine(">DDS|" + e.Message);
            omsLog.log.Info(">DDS|" + e.Message);
            string msg = e.Message;
            string command =  msg.Substring(0, msg.IndexOf('|')).ToLower();
            //代替了omsclients中的processmessage的功能
            if(msg.IndexOf('|') == -1 )
            {
                omsLog.log.Error("error command: " + msg);
                return;
            }
            if(command == "open" ||command == "openall" ||command == "close" ||command == "list" ||command == "session" ||command == "dump" ||command == "dumpfile")
            {
                //message from a subscriber
                //todo: 先实现openall|
                //e.Socket.SendMessage("on message i call you back");
                //非常关键的一点 根据remoteaddress来从clientlist中帅选回复的客户端。
                //MessageAction(e.Socket, e.Message);
                switch(command)
                {
                    case "open":
                        asOpen(e.Socket, e.Message);
                        break;
                    case "openall":
                        asOpenAll(e.Socket, e.Message);
                        break;
                    case "close":
                        asClose(e.Socket, e.Message);
                        break;
                    case "list":
                        asList(e.Socket, e.Message);
                        break;
                    case "session":
                        asSession(e.Socket, e.Message);
                        break;
                    case "dump":
                        asDumpAll(e.Socket, e.Message);
                        break;
                    case "dumpfile":
                        asDumpFile(e.Socket, e.Message);
                        break;
                    default:
                        break;
                }

                clientList[e.Socket.RemoteAddress].SendMessage("can i ?");
            }
            else if(command == "image" && command == "udpate" && command == "listsetup" && command == "reset")
            {
                //message from contributor
                //todo:先有contributor 再有subscriber
                switch (command)
                {
                    case "iamge":
                        asImage(e.Socket, e.Message);
                        break;
                    case "update":
                        asUpdate(e.Socket, e.Message);
                        break;
                    case "listsetup":
                        asListsetup(e.Socket, e.Message);
                        break;
                    case "reset":
                        asReset(e.Socket, e.Message);
                        break;
                    default:
                        break;
                }
                //this.currentClient = clientList[((TSocketClient)sender).];
                
            }
           // DDSServer.SendClientMsg("writeback: i have received you request!")
            //clientList.Add(e.
            
            //todo: 至少需要保持两个dict 1. 客户端根据名称来索引  2.信息根据各种类型特征来索引
        }

        
        private void DDSServer_OnClientConnect(object sender, SocketClientConnectEventArgs e)
        {
            omsLog.log.Debug("client connectted!");
            //((TSocketClient)sender).
            //client
            clientList[e.ClientID].SendMessage("call you back later\r\n");
            
        }


#region Action list for message 
        public bool MessageAction(TSocketClient socketclient, string msg)
        {
            //switch()
            
            return true;
        }

        public bool asClose(TSocketClient socketclient, string msg)
        {
            return true;
        }

        public bool asOpen(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asList(TSocketClient socketclient, string msg)
        {
            return true;
        }

        public bool asListsetup(TSocketClient socketclient, string msg)
        {
            return true;
        }

        public bool asStop(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asImage(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asUpdate(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asSession(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asSendAll(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asOpenAll(TSocketClient socketclient, string msg)
        {
            //需要注意两种方式both， 这样返回的只是静态的信息，每次更新的也要同步赚翻，才能cascade
            omsLog.log.Warn("openall for " + socketclient.RemoteAddress);
            foreach (string aMsg in this.openall)
            {
                clientList[socketclient.RemoteAddress].SendMessage(aMsg);
            }
            return true;
        }
        public bool asReset(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asImageNew(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asUpdateNew(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asDumpAll(TSocketClient socketclient, string msg)
        {
            return true;
        }
        public bool asDumpFile(TSocketClient socketclient, string msg)
        {
            return true;
        }
#endregion


    }


    //可以用直接用TSocketClient来代替
    class omsClient: TSocketClient
    {
        public string handle;

    }

    //也许需要这个class 也许把他的功能并入到server之中去
    class omsClients
    {
        public List<string> connections;

        public omsClients()
        {

        }

        //public void ProcessMessage(object sender; string msg)
        //{

        //}
    }
}
