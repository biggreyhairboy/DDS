using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Configuration;
using System.Collections.Specialized;
using System.Globalization;
using OMS.lib;
using OMS.common.Sockets;
using OMS.utils;

namespace OMS
{
    class DDSServer: TSocketServer 
    {
        public string serviceName;
        public TSocketClient currentClient;
        public string currentMsg;
        public List<string> openall;

        //store contribute images
        public List<string> SymbolsList;
        public Dictionary<string, string> SymbolImageDictionary;

        //store handles mapping relationships
        public Dictionary<string, List<string>> SymbolHandlesDictionary;
        public Dictionary<string, TSocketClient> HandleClientDictionary;

        //public Dictionary<string, oms>

        public DDSServer(string ip, int port):base(ip, port)
        {
            SymbolsList = new List<string>();
            SymbolImageDictionary = new Dictionary<string, string>();
            SymbolHandlesDictionary = new Dictionary<string, List<string>>();
            HandleClientDictionary = new Dictionary<string, TSocketClient>();

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
            if (!msg.Contains('|'))
            {
                return;                
            }
            string command =  msg.Substring(0, msg.IndexOf('|')).ToLower();
            //代替了omsclients中的processmessage的功能
            if(msg.IndexOf('|') == -1 )
            {
                omsLog.log.Error("error command: " + msg);
                return;
            }
            

            if (command == "image" || command == "udpate" || command == "listsetup" || command == "reset")
            {
                string currentsymbol = getSymbol(msg, DDSCommandType.Contribute);
                //message from contributor
                //todo:先有contributor 再有subscriber
                switch (command)
                {
                    case "image":
                        if (!SymbolImageDictionary.ContainsKey(currentsymbol))
                        {
                            asImageNew(e.Socket, e.Message, currentsymbol);
                        }
                        else
                        {
                            SymbolImageDictionary[currentsymbol] = e.Message;
                            asImage(e.Socket, e.Message, currentsymbol);
                        }

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
                }
            }else if (command == "open" || command == "openall" || command == "close" || command == "list" ||
                     command == "session" || command == "dump" || command == "dumpfile")
            {
                //message from a subscriber
                //todo: 先实现openall|
                //e.Socket.SendMessage("on message i call you back");
                //非常关键的一点 根据remoteaddress来从clientlist中帅选回复的客户端。
                //MessageAction(e.Socket, e.Message);
                string currenthandle = getHandle(msg);
                DDSCommandMode currentmode = getMode(msg);
                switch (command)
                {
                    case "open":
                        asOpen(e.Socket, e.Message, currenthandle, currentmode);
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
                }

                clientList[e.Socket.RemoteAddress].SendMessage("can i ?");
            }
            //this.currentClient = clientList[((TSocketClient)sender).];                           
           // DDSServer.SendClientMsg("writeback: i have received you request!")
            //clientList.Add(e.
            
            //todo: 至少需要保持两个dict 1. 客户端根据名称来索引  2.信息根据各种类型特征来索引
        }

        private DDSCommandMode getMode(string msg)
        {
            string tmp = msg.TrimEnd('|');
            string mode = tmp.Substring(tmp.LastIndexOf('|') + 1, tmp.Length - tmp.LastIndexOf('|') - 1);
            DDSCommandMode ddsCommandMode = DDSCommandMode.image;
            switch (mode)
            {
                case "image":
                    ddsCommandMode = DDSCommandMode.image;
                    break;
                case "upate":
                    ddsCommandMode = DDSCommandMode.update;
                    break;
                case "both":
                    ddsCommandMode = DDSCommandMode.both;
                    break;
            }
            return ddsCommandMode;
        }

        /// <summary>
        /// 根据命令的不同类型获取symbol的值
        /// </summary>
        /// <param name="tmpmsg">从socket获取的一条命令</param>
        /// <param name="commandType">命令的类型</param>
        /// <returns></returns>
        private string getSymbol(string tmpmsg, DDSCommandType commandType)
        {            
            if (commandType == DDSCommandType.Subscribe)
            {
                int commandLength = tmpmsg.IndexOf('|') + 1;
                string handlestr = tmpmsg.Substring(commandLength, tmpmsg.Length - commandLength);
                if (handlestr.Trim() != "")
                {
                    string handle = handlestr.Substring(0, handlestr.IndexOf('|'));
                    string symbolstring = handlestr.Substring(handle.Length + 1, handlestr.Length - handle.Length -1);
                    string symbol = symbolstring.Substring(0, symbolstring.IndexOf('|'));
                    return symbol;
                }
                else
                {
                    omsLog.log.Error(tmpmsg);
                    return "";
                }
            }
            else
            {
                int commandLength = tmpmsg.IndexOf('|') + 1;
                string symbolstring = tmpmsg.Substring(commandLength, tmpmsg.Length - commandLength);
                if (symbolstring.Trim() != "")
                {
                    string symbol = symbolstring.Substring(0, symbolstring.IndexOf('|'));
                    return symbol;
                }
                else
                {
                    omsLog.log.Error(tmpmsg);
                    return "";
                }
            }
          
        }

        private string getHandle(string msg)
        {
            int commandLength = msg.IndexOf('|') + 1;
            string handlestr = msg.Substring(commandLength, msg.Length - commandLength);
            if (handlestr.Trim() != "")
            {
                string handle = handlestr.Substring(0, handlestr.IndexOf('|'));
                return handle;
            }
            else
            {
                omsLog.log.Error(msg);
                return "";
            }
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

        public bool asOpen(TSocketClient socketclient, string msg, string handle, DDSCommandMode mode)
        {
            string symbol = getSymbol(msg, DDSCommandType.Subscribe);            
            if (SymbolImageDictionary.ContainsKey(symbol))
            {
                switch (mode)
                {
                    case DDSCommandMode.image:
                        clientList[socketclient.RemoteAddress].SendMessage(SymbolImageDictionary[symbol]);
                        break;
                    case DDSCommandMode.update:
                        SymbolHandlesDictionary.Add(symbol, new List<string>(){handle});
                        HandleClientDictionary.Add(handle, socketclient);
                        break;
                    case DDSCommandMode.both:
                        clientList[socketclient.RemoteAddress].SendMessage(SymbolImageDictionary[symbol]);
                        SymbolHandlesDictionary.Add(symbol, new List<string>() { handle });
                        HandleClientDictionary.Add(handle, socketclient);
                        break;
                    default:
                        return false;
                        break;
                }
                return true;
            }
            else
            {
                socketclient.SendMessage("image not available!");
                return false;
            }

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

        public bool asImage(TSocketClient socketclient, string msg, string symbol)
        {
            //string symb = msg.Substring(msg.IndexOf('|')+ 1, msg.Length - 1);
            if (SymbolHandlesDictionary.ContainsKey(symbol))
            {
                foreach (KeyValuePair<string, TSocketClient> kvp in HandleClientDictionary)
                {
                    kvp.Value.SendMessage(msg);
                    //或者用clientlists来发送推送信息
                    clientList[kvp.Value.RemoteAddress].SendMessage(msg);
                }
            }
            return true;
        }

        public bool asImageNew(TSocketClient socketclient, string msg, string symbol)
        {
            //todo：如果在image存在之前就已经有订阅了，是否需要推送, 暂时觉得不应该推送
            SymbolsList.Add(symbol);
            SymbolImageDictionary.Add(symbol, msg);
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
