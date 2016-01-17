using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OMS.common.Models.DataModel;

namespace OMS.common.Sockets
{
    public class ITSSynchonizer : IDisposable
    {
        public const string ITSVERIFY = "ITSVERIFY";
        public const string QUERYPOSITION = "QUERYPOSITION";
        public const string QUERYBODDATA = "QUERYBODDATA";
        public const string QUERYHISTORY = "QUERYHISTORY";
        public const string QUERYSUBORDER = "QUERYSUBORDER";
        public const string QUERYTRADE = "QUERYTRADE";
        public const string CUSTOMQUERY = "CUSTOMQUERY";
        public const string ORD = "ORD";
        public const string CALCULATE = "CALCULATE";
        public const string SETTLEMENT = "SETTLEMENT";

        public event EventHandler<SocketReceiveEventArgs> OnMessage;
        public event EventHandler<SocketErrorEventArgs> OnError;
        public event EventHandler<SocketStatusEventArgs> OnState;

        protected IOmsSynchonizer sync;
        protected OrderModelBase orderModel;
        protected bool verified;
        protected bool isDisposed;
        protected int timeout;

        public ITSSynchonizer(int timeout, OrderModelBase orderModel)
        {
            this.orderModel = orderModel;
            sync = new SynchonizerBase();
            sync.OnMessage += new EventHandler<SocketReceiveEventArgs>(sync_OnMessage);
            sync.OnError += new EventHandler<SocketErrorEventArgs>(sync_OnError);
            sync.OnState += new EventHandler<SocketStatusEventArgs>(sync_OnState);
            this.timeout = timeout;
        }

        private void sync_OnState(object sender, SocketStatusEventArgs e)
        {
            if (OnState != null) OnState(sender, e);
        }

        private void sync_OnError(object sender, SocketErrorEventArgs e)
        {
            if (OnError != null) OnError(sender, e);
        }

        private void sync_OnMessage(object sender, SocketReceiveEventArgs e)
        {
            if (OnMessage != null) OnMessage(sender, e);
        }

        public Exception LastError { get { return sync.LastError; } }

        public bool IsVerified { get { return verified; } }

        public bool IsDisposed { get { return isDisposed; } }

        public string Host { get { return sync.Host; } }

        public int Port { get { return sync.Port; } }

        public bool Connect(string host, int port)
        {
            return sync.Connect(host, port, timeout);
        }

        public bool Verify(string cmd)
        {
            OmsRequest request = new OmsRequest(cmd, ITSVERIFY, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessITSVerify);
            return sync.SendRequest(request);
        }

        public bool QueryPosition(string cmd, ref List<string> positions)
        {
            positions = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, QUERYPOSITION, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessPositions);
            bool res = sync.SendRequest(request);
            positions = request.CustomData as List<string>;
            return res;
        }

        public bool QueryBodData(string cmd, ref List<string> bodData)
        {
            bodData = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, QUERYBODDATA, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessBODData);
            bool res = sync.SendRequest(request);
            bodData = request.CustomData as List<string>;
            return res;
        }

        public bool QueryHistory(string cmd, ref List<string> historyData)
        {
            historyData = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, QUERYHISTORY, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessHistoryData);
            bool res = sync.SendRequest(request);
            historyData = request.CustomData as List<string>;
            return res;
        }

        public bool QuerySuborder(string cmd, ref List<string> suborders)
        {
            suborders = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, QUERYSUBORDER, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessSuborderQuery);
            bool res = sync.SendRequest(request);
            suborders = request.CustomData as List<string>;
            return res;
        }

        public bool QueryTrade(string cmd, ref List<string> trades)
        {
            trades = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, QUERYTRADE, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessTradeQuery);
            bool res = sync.SendRequest(request);
            trades = request.CustomData as List<string>;
            return res;
        }

        public bool CustomQuery(string cmd, ref List<string> results)
        {
            results = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, CUSTOMQUERY, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessCustomQuery);
            request.Validate = new MsgFilter(CustomQueryValidationCheck);
            bool res = sync.SendRequest(request);
            results = request.CustomData as List<string>;
            return res;
        }

        public bool RedirectOrderCommand(string cmd, ref List<string> response)
        {
            response = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, ORD, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessOrderResponse);
            request.Validate = new MsgFilter(RedirectOrderCheck);
            TOMSMessage omsMsg = new TOMSMessage();
            omsMsg.CreateFromCommand(cmd);
            OrderCheckPoint ocp = new OrderCheckPoint();
            if (omsMsg.ContainsKey("6"))
            {
                ocp.OrderNo = omsMsg.GetAttribute("6");
            }
            if (cmd.StartsWith("ORD|add"))
            {
                ocp.Command = "add";
            }
            else if (cmd.StartsWith("ORD|cancel"))
            {
                ocp.Command = "cancel";
            }
            else if (cmd.StartsWith("ORD|change"))
            {
                ocp.Command = "change";
            }
            request.CheckPoint = ocp;
            bool res = sync.SendRequest(request);
            response = request.CustomData as List<string>;
            return res;
        }

        public bool OrderCalculate(string cmd, ref List<string> response)
        {
            response = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, CALCULATE, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessOrderCalc);
            bool res = sync.SendRequest(request);
            response = request.CustomData as List<string>;
            return res;
        }

        public bool SendSettlement(string cmd, ref List<string> response)
        {
            response = null;
            if (!verified) return false;
            OmsRequest request = new OmsRequest(cmd, SETTLEMENT, timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessSettlement);
            bool res = sync.SendRequest(request);
            response = request.CustomData as List<string>;
            return res;
        }

        public void SendITSMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            sync.SendRequestAsync(msg);
        }

        protected void ProcessSettlement(object sender, RequestEventArgs e)
        {
            e.Request.Done = true;
            if (e.Message.StartsWith("sendmessage|"))
            {
                e.Request.Success = true;
                List<string> data = e.Request.CustomData as List<string>;
                if (data == null)
                {
                    data = new List<string>();
                    data.Add(e.Message);
                    e.Request.CustomData = data;
                }
                else
                {
                    //discard
                }
            }
            else
            {
                if (e.Message.StartsWith("error"))
                {
                    e.Request.LastError = new Exception(e.Message);
                }
            }
        }

        protected void ProcessOrderCalc(object sender, RequestEventArgs e)
        {
            e.Request.Done = true;
            if (e.Message.StartsWith("image|CALCD"))
            {
                e.Request.Success = true;
                List<string> data = e.Request.CustomData as List<string>;
                if (data == null)
                {
                    data = new List<string>();
                    e.Request.CustomData = data;
                }
                data.Add(e.Message);
            }
            else
            {
                if (e.Message.StartsWith("error"))
                {
                    e.Request.LastError = new Exception(e.Message);
                }
            }
        }

        protected void ProcessOrderResponse(object sender, RequestEventArgs e)
        {
            e.Request.Done = true;
            if (e.Message.StartsWith("image|ORD") || e.Message.StartsWith("image|ERRORD_"))
            {
                e.Request.Success = true;
                if (e.Message.StartsWith("image|ERROD_")) e.Request.Success = false;
                List<string> data = e.Request.CustomData as List<string>;
                if (data == null)
                {
                    data = new List<string>();
                    data.Add(e.Message);
                    e.Request.CustomData = data;
                }
                else
                {
                    //discard the update
                }
            }
            else
            {
                if (e.Message.StartsWith("error"))
                    e.Request.LastError = new Exception(e.Message);
            }
        }

        protected void CustomQueryValidationCheck(ValidateInfo info, FilterCallback callback)
        {
            info.Accessible = false;
            if (!info.Request.Obsolete)
            {
                if (info.Message != null && info.Message.Trim() != "")
                {
                    if (info.Message.StartsWith("error|"))
                    {
                        info.Accessible = info.Message.ToUpper().Contains("|9|CUSTOMQUERY|");
                    }
                    else
                    {
                        TOMSMessage omsMsg = new TOMSMessage();
                        omsMsg.CreateFromCommand(info.Message);
                        string queryName = "";
                        if (omsMsg.ContainsKey("457"))
                            queryName = omsMsg.GetAttribute("457");
                        if (queryName == null || queryName.Trim() == "")
                        {
                            if (omsMsg.ContainsKey("74"))
                                queryName = omsMsg.GetAttribute("74");
                        }
                        if (queryName != null && queryName.Trim() != "")
                        {
                            string origQueryName = "";
                            omsMsg.CreateFromCommand(info.Request.Request);
                            if (omsMsg.ContainsKey("457"))
                                origQueryName = omsMsg.GetAttribute("457");
                            if (origQueryName == null || origQueryName.Trim() == "")
                            {
                                if (omsMsg.ContainsKey("74"))
                                    origQueryName = omsMsg.GetAttribute("74");
                            }

                            if (origQueryName != null && origQueryName.Trim() != "")
                                info.Accessible = queryName == origQueryName;
                        }
                    }
                }
            }
            callback(info);
        }

        protected void RedirectOrderCheck(ValidateInfo info, FilterCallback callback)
        {
            info.Accessible = false;
            if (!info.Request.Obsolete && (info.Message.StartsWith("image|ORD") || info.Message.StartsWith("image|ERRORD")))
            {
                if (info.Request.CheckPoint != null)
                {
                    OrderCheckPoint ocp = info.Request.CheckPoint as OrderCheckPoint;
                    if (ocp != null)
                    {
                        bool operationMatch = info.Message.Contains(string.Format("|187|{0}|", ocp.Command));
                        if (operationMatch)
                        {
                            TOMSMessage omsMsg = new TOMSMessage();
                            omsMsg.CreateFromCommand(info.Message);
                            bool validData = false;
                            string orderNo = "";
                            if (omsMsg.ContainsKey("6"))
                            {
                                orderNo = omsMsg.GetAttribute("6");
                                if (orderNo != null && orderNo.Trim() != "")
                                {
                                    validData = true;
                                }
                            }
                            if (validData)
                            {
                                if (ocp.Command == "add")
                                {
                                    if (orderModel != null)
                                    {
                                        orderModel.AcquireOrderLock();
                                        try
                                        {
                                            info.Accessible = !orderModel.HasOrder(orderNo);//bypass the order update
                                        }
                                        finally
                                        {
                                            orderModel.ReleaseOrderLock();
                                        }
                                    }
                                }
                                else
                                {
                                    info.Accessible = ocp.OrderNo == orderNo;
                                }
                            }
                            else
                            {
                                if (ocp.Command == "add" && info.Message.StartsWith("image|ERRORD_"))
                                {
                                    info.Accessible = true;
                                }
                            }
                        }
                    }
                }
            }
            callback(info);
        }

        protected void ProcessCustomQuery(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^customquery\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessTradeQuery(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^querytrade\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessSuborderQuery(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^querysuborder\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessHistoryData(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^history\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessBODData(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^boddata\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessPositions(object sender, RequestEventArgs e)
        {
            if (e.Request.LastError != null)
            {
                e.Request.Done = true;
                return;
            }
            ProcessSequenceData(e.Request, e.Message, @"^position\|(?<current>.*?)/(?<total>.*?)\|.*$");
        }

        protected void ProcessSequenceData(OmsRequest request, string msg, string pattern)
        {
            Regex regex = new Regex(pattern);
            Match m = regex.Match(msg);
            if (m.Success)
            {
                string current = m.Groups["current"].Value;
                string total = m.Groups["total"].Value;
                if (current == total)
                {
                    request.Done = true;
                    request.Success = true;
                }
                List<string> data = request.CustomData as List<string>;
                if (data == null)
                {
                    data = new List<string>();
                    request.CustomData = data;
                }
                data.Add(msg);
            }
            else
            {
                if (msg.StartsWith("error|"))
                {
                    switch (request.RequestID)
                    {
                        case CUSTOMQUERY:
                            if (msg.ToUpper().Contains("CUSTOMQUERY")) request.Done = true;
                            return;
                        case QUERYBODDATA:
                            if (msg.ToUpper().Contains("BODDATA")) request.Done = true;
                            return;
                        case QUERYSUBORDER:
                            if (msg.ToUpper().Contains("QUERYSUBORDER")) request.Done = true;
                            return;
                        case QUERYTRADE:
                            if (msg.ToUpper().Contains("QUERYTRADE")) request.Done = true;
                            return;
                        default:
                            break;
                    }
                }
            }
        }

        protected void ProcessITSVerify(object sender, RequestEventArgs e)
        {
            e.Request.Done = true;
            if (e.Message.StartsWith("session"))
            {
                e.Request.Success = true;
                verified = true;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            if (sync != null)
                sync.Dispose();
        }

        #endregion
    }

    public class OrderCheckPoint
    {
        protected string cmd;
        protected string orderNo;
        protected int instruct;
        protected int instruct1;

        public OrderCheckPoint() { }

        public string Command { get { return cmd; } set { cmd = value; } }

        public string OrderNo { get { return orderNo; } set { orderNo = value; } }

        public int Instruct { get { return instruct; } set { instruct = value; } }

        public int Instruct1 { get { return instruct1; } set { instruct1 = value; } }
    }
}
