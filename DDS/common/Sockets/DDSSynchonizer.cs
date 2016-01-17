using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;

namespace OMS.common.Sockets
{
    public class DDSSynchonizer
    {
        protected IOmsSynchonizer sync;
        protected int timeout;

        public DDSSynchonizer(int timeout)
        {
            sync = new SynchonizerBase();
            this.timeout = timeout;
        }

        public Exception LastError { get { return sync.LastError; } }

        public bool Connect(string host, int port)
        {
            return sync.Connect(host, port, timeout);
        }

        public bool SubscribeSymbol(string symbol, ref SubscribeResult response)
        {
            return SubscribeSymbol(symbol, DDSMode.dmImage, ref response);
        }
        /// <summary>
        /// Subscribe symbol in modes
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mode"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>Function changes to protected, not allow to subscribe in BOTH mode, since the update message will cause issue in synchonization</remarks>
        protected bool SubscribeSymbol(string symbol, DDSMode mode, ref SubscribeResult response)
        {
            response = null;
            OmsRequest request = new OmsRequest(string.Format("open|{0}|{0}|mode|{1}|", symbol, OmsHelper.GetDDSMode(mode)), "SUBSCRIBESYMBOL", timeout);
            request.Handle = new EventHandler<RequestEventArgs>(ProcessSymbolCallback);
            bool res = sync.SendRequest(request);
            response = request.CustomData as SubscribeResult;
            return res;
        }

        protected void ProcessSymbolCallback(object sender, RequestEventArgs e)
        {
            e.Request.Done = true;
            if (e.Message.StartsWith("image|"))
            {
                e.Request.Success = true;
                SubscribeResult sr = e.Request.CustomData as SubscribeResult;
                if (sr == null)
                {
                    sr = new SubscribeResult();
                    sr.ProcessMessage(e.Message);
                    e.Request.CustomData = sr;
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
    }
}
