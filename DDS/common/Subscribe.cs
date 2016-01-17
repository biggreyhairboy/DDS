using System;
using System.Collections.Generic;
using OMS.common.Sockets;
using OMS.common.Utilities;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OMS.common
{
    public class SubscribeResultEventArgs : EventArgs
    {
        protected SubscribeResult res;

        public SubscribeResultEventArgs(SubscribeResult res)
        {
            this.res = res;
        }

        public SubscribeResult Result { get { return res; } }
    }

    public class SubscribeGroupEventArgs : EventArgs
    {
        protected Dictionary<string, string> res;
        public SubscribeGroupEventArgs(Dictionary<string, string> res)
        {
            this.res = res;
        }

        public Dictionary<string, string> Result { get { return res; } }
    }

    public class SubscribeResult
    {
        public event EventHandler OnValidationChanged;

        protected string symbol;
        protected string dataType;
        protected string handler;

        protected OmsItem subItem;
        protected event EventHandler<SubscribeResultEventArgs> onInternalData;
        protected bool isValid;

        public SubscribeResult()
        {
            subItem = new OmsItem();
        }

        public string Symbol
        {
            get { return symbol; }
            internal set { symbol = value; }
        }

        public string Handler
        {
            get { return handler; }
            internal set { handler = value; }
        }

        public bool IsValid
        {
            get { return isValid; }
            internal set
            {
                if (isValid != value)
                {
                    isValid = value;
                    FireOnValidationChanged();
                }
            }
        }

        private void FireOnValidationChanged()
        {
            if (OnValidationChanged != null)
            {
                if (omsCommon.SyncInvoker == null)
                    OnValidationChanged(this, EventArgs.Empty);
                else omsCommon.SyncInvoker.Invoke(OnValidationChanged, new object[] { this, EventArgs.Empty });
            }
        }

        private void FireOnInternalData()
        {
            if (onInternalData != null)
            {
                if (omsCommon.SyncInvoker == null)
                    onInternalData(this, new SubscribeResultEventArgs(this));
                else omsCommon.SyncInvoker.Invoke(onInternalData, new object[] { this, new SubscribeResultEventArgs(this) });
            }
        }

        internal void ProcessMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            if (subItem == null) subItem = new OmsItem();
            subItem.Symbol = symbol;
            subItem.ParseFrom(msg);
            IsValid = true;
            FireOnInternalData();
        }

        public override string ToString()
        {
            return subItem.ToString();
        }

        public bool ContainsKey(int key)
        {
            return subItem.ContainsKey(key);
        }

        public bool ContainsKey(string key)
        {
            return subItem.ContainsKey(key);
        }

        public string GetAttributeAsString(string key)
        {
            return subItem.GetValue(key);
        }

        public string GetAttributeAsString(int key)
        {
            return subItem[key];
        }

        public int GetAttributeAsInteger(string key)
        {
            int res = 0;
            int.TryParse(subItem.GetValue(key), out res);
            return res;
        }

        public int GetAttributeAsInteger(int key)
        {
            int res = 0;
            int.TryParse(subItem.GetValue(key), out res);
            return res;
        }

        public decimal GetAttributeAsDecimal(string key)
        {
            return subItem.GetDecimalValue(key);
        }

        public decimal GetAttributeAsDecimal(int key)
        {
            return subItem.GetDecimalValue(key);
        }

        public void AddHandler(EventHandler<SubscribeResultEventArgs> handler)
        {
            if (handler == null) return;
            onInternalData += handler;
            if (isValid)
                if (omsCommon.SyncInvoker == null)
                    handler(this, new SubscribeResultEventArgs(this));
                else omsCommon.SyncInvoker.Invoke(handler, new object[] { this, new SubscribeResultEventArgs(this) });
        }

        public void RemoveHandler(EventHandler<SubscribeResultEventArgs> handler)
        {
            if (handler == null) return;
            onInternalData -= handler;
        }

        public void ResetHandler()
        {
            onInternalData = null;
        }
    }

    public class SubscribeList
    {
        protected string groupID;
        protected string nextLink;
        protected bool isValid;
        protected Dictionary<string, string> subList;
        protected event EventHandler<SubscribeGroupEventArgs> onInternalData;

        public string GroupID
        {
            get { return groupID; }
            internal set { groupID = value; }
        }
        public string NextLink
        {
            get { return nextLink; }
            internal set { nextLink = value; }
        }
        public bool Isvalid
        {
            get { return isValid; }
            internal set { isValid = value; }
        }

        public SubscribeList()
        {
            groupID = "";
            nextLink = "";
            subList = new Dictionary<string, string>();
        }

        private void FireOnInternalData()
        {
            if (onInternalData != null)
            {
                if (omsCommon.SyncInvoker == null)
                    onInternalData(this, new SubscribeGroupEventArgs(subList));
                else omsCommon.SyncInvoker.Invoke(onInternalData, new object[] { this, new SubscribeGroupEventArgs(subList) });
            }
        }

        internal void ProcessMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            OmsItem item = new OmsItem();
            item.ParseFrom(msg);
            for (int i = 0; i<=19; i++)
            {
                string tempSymbol = item[omsConst.OMS_SYMBOL_LIST * 100 + i];
                if (tempSymbol.Length > 0)
                    subList.Add(tempSymbol, tempSymbol);
            }
            nextLink = item[omsConst.OMS_SYMBOL_LIST * 100 + 20];
            if (nextLink.Length <= 0)
            {
                isValid = true;
                FireOnInternalData();
            }
        }

        public void AddHandler(EventHandler<SubscribeGroupEventArgs> handler)
        {
            if (handler == null) return;
            onInternalData += handler;
            if (isValid)
            {
                if (omsCommon.SyncInvoker == null)
                    handler(this, new SubscribeGroupEventArgs(subList));
                else omsCommon.SyncInvoker.Invoke(handler, new object[] { this, new SubscribeGroupEventArgs(subList) });
            }
        }

        public void RemoveHandler(EventHandler<SubscribeGroupEventArgs> handler)
        {
            if (handler == null) return;
            onInternalData -= handler;
        }


        public void ResetHandler()
        {
            onInternalData = null;
        }

    }

    public class SubscribeListResult
    {
        protected int keyTag;
        protected string listID;
        protected Dictionary<string, SubscribeResult> results;
        protected event EventHandler<SubscribeResultEventArgs> onInternalData;

        public SubscribeListResult()
        {
            listID = "";
            keyTag = omsConst.OMS_SYMBOL;
            results = new Dictionary<string, SubscribeResult>();
            onInternalData = null;
        }

        public string ListID
        {
            get { return listID; }
            internal set { listID = value; }
        }

        public int KeyTag
        {
            get { return keyTag; }
            set { keyTag = value; }
        }

        private void FireOnInternalData(object sender, SubscribeResultEventArgs e)
        {
            if (onInternalData != null)
            {
                if (omsCommon.SyncInvoker == null)
                    onInternalData(sender, e);
                else omsCommon.SyncInvoker.Invoke(onInternalData, new object[] { sender, e });
            }
        }

        public void HandleSubItemUpdate(object sender, SubscribeResultEventArgs e)
        {
            FireOnInternalData(sender, e);
        }

        public void AddHandler(EventHandler<SubscribeResultEventArgs> handler)
        {
            if (handler == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(results);
            try
            {
                onInternalData += handler;
                if (results.Count > 0)
                {
                    foreach (SubscribeResult item in results.Values)
                    {
                        if (omsCommon.SyncInvoker == null)
                            handler(this, new SubscribeResultEventArgs(item));
                        else omsCommon.SyncInvoker.Invoke(handler, new object[] { this, new SubscribeResultEventArgs(item) });
                    }
                }
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(results);
            }
        }

        public void RemoveHandler(EventHandler<SubscribeResultEventArgs> handler)
        {
            if (handler == null) return;
            onInternalData -= handler;
        }

        public void ResetHandler()
        {
            onInternalData = null;
            if (results != null)
            {
                foreach (SubscribeResult item in results.Values)
                {
                    item.ResetHandler();
                }
            }
        }

        internal void ProcessMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            OmsItem item = new OmsItem();
            item.ParseFrom(msg);
            string key = item[KeyTag];
            SubscribeResult res = null;
            if (results.ContainsKey(key))
            {
                res = results[key];
            }
            else
            {
                res = new SubscribeResult();
                res.Symbol = item[omsConst.OMS_SYMBOL];
                res.AddHandler(new EventHandler<SubscribeResultEventArgs>(HandleSubItemUpdate));
                results[key] = res;
            }
            res.ProcessMessage(msg);
        }

        internal void ResetData()
        {
            if (results == null) return;
            omsCommon.AcquireSyncLock(results);
            try
            {
                results.Clear();
            }
            finally
            {
                omsCommon.ReleaseSyncLock(results);
            }
        }
    }

    public class SubscribeManager : IDisposable
    {
        private static Dictionary<string, SubscribeManager> submanList;
        private static SubscribeManager instance;
        private static volatile object syncRoot = new object();
        private   bool ddsVerified = true;
        private const string verifydds_backHandle = "system";

        protected const string PriceHandle = "PRICE";
        protected const string SymbolHandle = "SYMBOL";
        protected const string ListHandle = "LIST";
        protected const string GroupHandle = "GROUP";
        protected Dictionary<string, SubscribeResult> symbols;
        protected Dictionary<string, SubscribeList> groups;
        protected Dictionary<string, SubscribeListResult> listData;
        protected TSocketClient sockClient;
        protected AutoResetEvent signal;
        protected string name;
        protected Regex regex;

        public delegate void HandleRequestVerify();
        public event HandleRequestVerify  OnRequestVerify;

        public SubscribeManager(TSocketClient sock)
        {
            symbols = new Dictionary<string, SubscribeResult>();
            groups = new Dictionary<string, SubscribeList>();
            listData = new Dictionary<string, SubscribeListResult>();
            regex = new Regex(@"\|505\|(?<value>.*?)\|", RegexOptions.Singleline);
            signal = new AutoResetEvent(false);
            this.sockClient = sock;
            sockClient.OnSocketBinary += new EventHandler<BinaryReceiveEventArgs>(sockClient_OnSocketBinary);
            sockClient.OnSocketMessage += new EventHandler<SocketReceiveEventArgs>(HandleSocketMsg);
            sockClient.OnError += new EventHandler<SocketErrorEventArgs>(HandleError);
            sockClient.OnSocketStatus += new EventHandler<SocketStatusEventArgs>(HandleStatus);
        }

        public SubscribeManager(TSocketClient sock, string name)
            : this(sock)
        {
            this.name = name;
        }

        public static void SetInstance(SubscribeManager item)
        {
            if (item == null) return;
            instance = item;
            AddSubscribeManager(item);
        }

        public static SubscribeManager Instance { get { return instance; } }

        public static SubscribeManager InstanceOf(string name)
        {
            if (submanList == null) return null;
            if (name == null || name.Trim() == "") return null;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(syncRoot);
            try
            {
                if (submanList.ContainsKey(name))
                    return submanList[name];
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(syncRoot);
            }
            return null;
        }

        internal static void AddSubscribeManager(SubscribeManager item)
        {
            if (item == null) return;
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(syncRoot);
            try
            {
                if (submanList == null)
                    submanList = new Dictionary<string, SubscribeManager>();
                if (item.Name == null)
                    item.Name = item.sockClient.RemoteAddress;
                if (!submanList.ContainsKey(item.Name))
                {
                    submanList.Add(item.Name, item);
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(syncRoot);
            }
        }

        public string Name
        {
            get { return name; }
            internal set { name = value; }
        }
        /// <summary>
        /// Should be called only when socket reconnect, for the need of resubscribe data
        /// </summary>
        public void ClearData()
        {
            try
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(symbols);
                try
                {
                    symbols.Clear();
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(symbols);
                }

                if(omsCommon.SyncInvoker==null)
                    System.Threading.Monitor.Enter(groups);
                try
                {
                    groups.Clear();
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(groups);
                }

                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(listData);
                try
                {
                    listData.Clear();
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(listData);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #region "Clear Specified data"
        public void ClearSymbolData(string symbol)
        {
            try
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(symbols);
                try
                {
                    symbols.Remove(symbol);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(symbols);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public void ClearGroupData(string groupID)
        {
            try
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(groups);
                try
                {
                    groups.Remove(groupID);
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(groups);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }
        #endregion

        public SubscribeResult SubscribePrice(string symbol)
        {
            return SubscribeBy(symbol, PriceHandle);
        }

        public SubscribeResult SubscribeBySymbol(string symbol)
        {
            return SubscribeBy(symbol, SymbolHandle);
        }

        public SubscribeResult SubscribeImageBySymbol(string symbol)
        {
            return SubscribeImageBy(symbol, SymbolHandle);
        }

        internal SubscribeResult SubscribeImageBy(string symbol, string handler)
        {
            RequestVerify();
            if (symbol == null || symbol.Trim() == "") return null;
            if (handler == null || handler == "") handler = SymbolHandle;

            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(symbols);
            try
            {
                SubscribeResult res = null;
                if (!symbols.ContainsKey(symbol))
                {
                    res = new SubscribeResult();
                    res.Symbol = symbol;
                    res.Handler = handler;
                    symbols.Add(symbol, res);
                    SubscribeImage(symbol, handler);
                }
                else res = symbols[symbol];
                return res;
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
                return null;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(symbols);
            }
        }

        internal SubscribeResult SubscribeBy(string symbol, string handler)
        {
            RequestVerify();
            if (symbol == null || symbol.Trim() == "") return null;
            if (handler == null || handler == "") handler = SymbolHandle;

            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(symbols);
            try
            {
                SubscribeResult res = null;
                if (!symbols.ContainsKey(symbol))
                {
                    res = new SubscribeResult();
                    res.Symbol = symbol;
                    res.Handler = handler;
                    symbols.Add(symbol, res);
                    SubscribeTo(symbol, handler);
                }
                else res = symbols[symbol];
                return res;
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
                return null;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(symbols);
            }
        }

        internal void RequestVerify()
        {
            try
            {
                if (ddsVerified) return;
                else
                {
                    if (null != OnRequestVerify)
                        OnRequestVerify();
                    return;
                }

            }
            catch { }
        }

        protected void SubscribeTo(string symbol, string handler)
        {
            if (symbol == null || symbol.Trim() == "") return;
            if (handler == null || handler.Trim() == "") handler = SymbolHandle;
            SendDDSMessage(string.Format("open|{0}_{1}|{2}|mode|both|", handler, symbol, symbol));
        }

        protected void SubscribeImage(string symbol, string handler)
        {
            if (symbol == null || symbol.Trim() == "") return;
            if (handler == null || handler.Trim() == "") handler = SymbolHandle;
            SendDDSMessage(string.Format("open|{0}_{1}|{2}|mode|image|", handler, symbol, symbol));
        }

        protected void SubscribeToList(string listID)
        {
            if (listID == null || listID.Trim() == "") return;
            SendDDSMessage(string.Format("list|{0}_{1}|{2}|mode|both|", ListHandle, listID, listID));
        }

        public SubscribeList SubscribeGroup(string groupID)
        {
            RequestVerify();
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(groups);
            try
            {
                SubscribeList res = null;
                if (!groups.ContainsKey(groupID))
                {

                    res = new SubscribeList();
                    res.GroupID = groupID;
                    groups[groupID] = res;
                    SendDDSMessage(string.Format("open|{0}_{1}_GROUP|{2}|mode|image|", GroupHandle, groupID, groupID));
                }
                else
                {
                    res = groups[groupID];
                }
                return res;
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
                return null;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(groups);
            }
        }

        public SubscribeListResult SubscribeByList(string listID)
        {
            RequestVerify();
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(listData);
            try
            {
                SubscribeListResult res = null;
                if (!listData.ContainsKey(listID))
                {
                    res = new SubscribeListResult();
                    res.ListID = listID;
                    listData[listID] = res;
                    SubscribeToList(listID);
                }
                else
                {
                    res = listData[listID];
                }
                return res;
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
                return null;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(listData);
            }
        }

        protected void sockClient_OnSocketBinary(object sender, BinaryReceiveEventArgs e)
        {
            string msg = "";
            try
            {
                string big5Msg = Encoding.GetEncoding("BIG5").GetString(e.Buffer, e.Offset, e.Size);
                msg = big5Msg;
                string gb2312Msg = Encoding.GetEncoding("GB2312").GetString(e.Buffer, e.Offset, e.Size);
                List<string> multiEntries = new List<string>();
                for (Match m = regex.Match(gb2312Msg); m.Success; m = m.NextMatch())
                {
                    multiEntries.Add(m.Groups["value"].Value);
                }

                if (multiEntries.Count == 1)
                {
                    msg = regex.Replace(big5Msg, string.Format("|505|{0}|", multiEntries[0]));
                }
                else if (multiEntries.Count > 1)
                {
                    //bad matches, just drop it, do nothing
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                if (msg == null || msg.Trim() == "")
                    msg = Encoding.Default.GetString(e.Buffer, e.Offset, e.Size);
            }
            HandleSocketMsg(sender, new SocketReceiveEventArgs(msg));
        }

        protected void HandleSocketMsg(object sender, SocketReceiveEventArgs e)
        {
            try
            {
                if (omsCommon.LogPrice)
                    TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);//Leo.W: if log price, then all the message receiced from DDS should be log down
                OmsParser parser = new OmsParser(e.Message, "|");
                string command = "";
                string handle = "";
                if (parser.Next(ref command) && parser.Next(ref handle))
                {
                    if ((command == "image") || (command == "update"))
                    {
                        int index = handle.IndexOf("_");
                        if (index < 1)
                        {
                            TLog.DefaultInstance.WriteLog("Message handler miss underscore...", LogType.ERROR);
                            TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.ERROR);
                            return;
                        }
                        string msgHandle = handle.Substring(0, index);
                        string msgSymbol = handle.Substring(index + 1, handle.Length - index - 1);

                        //Leo.W: comment it out, just skip all messages even it's not price message
                        //if (!omsCommon.LogPrice)
                        //{
                        //    //If skip price log, the non-price info also should be log down
                        //    if (msgHandle != PriceHandle)
                        //        TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);
                        //}

                        if ((msgHandle == SymbolHandle) || (msgHandle == PriceHandle))
                        {
                            if (symbols.ContainsKey(msgSymbol))
                            {
                                SubscribeResult item = symbols[msgSymbol];
                                item.ProcessMessage(e.Message);
                            }
                        }
                        else if (msgHandle == ListHandle)
                        {
                            msgSymbol = msgSymbol.Substring(0, msgSymbol.LastIndexOf("_"));
                            if (listData.ContainsKey(msgSymbol))
                            {
                                SubscribeListResult listItem = listData[msgSymbol];
                                listItem.ProcessMessage(e.Message);
                            }
                        }
                        else if (msgHandle == GroupHandle)
                        {
                            msgSymbol = msgSymbol.Substring(0, msgSymbol.IndexOf("_"));
                            if (groups.ContainsKey(msgSymbol))
                            {
                                SubscribeList groupItem = groups[msgSymbol];
                                groupItem.ProcessMessage(e.Message);
                                if(groupItem.NextLink.Length>0)
                                    SendDDSMessage(string.Format("open|{0}|{1}|mode|image|", groupItem.NextLink, groupItem.GroupID));
                            }
                        }
                        else
                        {
                            TLog.DefaultInstance.WriteLog("Invalid message handler [" + msgHandle + "]", LogType.ERROR);
                        }
                    }
                    else if (command == "error")
                    {
                        string tag="";
                        string value="";
                        int id=0;
                        int errorcode = 0;
                        while (parser.Next(ref tag) && parser.Next(ref value))
                        {
                            if (int.TryParse(tag, out id)&&int .TryParse(value ,out errorcode ))
                            {
                                if (omsConst.OMS_ERRORCODE == id)
                                {
                                    switch (errorcode)
                                    {
                                        case OmsErrorConst.OMS_SSMERR_VERIFYFAILED:
                                            lock (syncRoot) ddsVerified = false;
                                            signal.Set();
                                            break;
                                        case OmsErrorConst.OMS_SSMERR_NEWSESSION:
                                        case OmsErrorConst.OMS_SSMERR_SESSIONKICK:
                                        case OmsErrorConst.OMS_SSMERR_SESSIONEXPIRE:
                                            lock (syncRoot) ddsVerified = false;
                                            break;
                                        case OmsErrorConst.OMS_PERMDENIED:
                                            if (handle == verifydds_backHandle)
                                                lock (syncRoot) ddsVerified = false;
                                            signal.Set();
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                            }
                        }
                        if (!omsCommon.LogPrice)//Only consider the case that if skip price log, the "error" messages should not be skip, and please be aware, if LogPrice's turn on, this message has been logged above, so no need to be log again
                            TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);
                    }
                    else if (command == "verified")
                    {
                        signal.Set();
                        lock (syncRoot) ddsVerified = true;
                        if (!omsCommon.LogPrice)
                            TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);
                        HandleStatus(null, new SocketStatusEventArgs(true));
                    }
                    else if (!omsCommon.LogPrice) TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);
                }
                else if (!omsCommon.LogPrice) TLog.DefaultInstance.WriteLog("DDS>|" + e.Message, LogType.INFO);
            }
            catch (Exception Ex)
            {
                TLog.DefaultInstance.WriteLog(Ex.ToString(), LogType.ERROR);
            }
        }

        public void SendDDSMessage(string msg)
        {
            try
            {
                sockClient.SendMessage(msg);
                TLog.DefaultInstance.WriteLog(">DDS|" + msg, LogType.INFO);
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        public void VerifyDDS(string msg, int millisecondsTimeout)
        {
            SendDDSMessage(msg);
            if (millisecondsTimeout > 0)
            {
                if (!signal.WaitOne(millisecondsTimeout, false))
                {
                    TLog.DefaultInstance.WriteLog(string.Format("SubscribeManager verify DDS sync timeout", msg));
                }
            }            
        }

        private void HandleError(object sender, SocketErrorEventArgs e)
        {
            TLog.DefaultInstance.WriteLog(e.LastError.ToString(), LogType.ERROR);
        }

        private void HandleStatus(object sender, SocketStatusEventArgs e)
        {
            if (!e.Connected)
            {
                lock (syncRoot) ddsVerified = false;
            }
            if (!ddsVerified) return;
            if (e.Connected)
            {
                if (symbols != null)
                {
                    omsCommon.AcquireSyncLock(symbols);
                    try
                    {
                        if (symbols.Count > 0)
                        {
                            foreach (SubscribeResult item in symbols.Values)
                            {
                                SubscribeTo(item.Symbol, item.Handler);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    }
                    finally
                    {
                        omsCommon.ReleaseSyncLock(symbols);
                    }
                }

                if (listData != null)
                {
                    omsCommon.AcquireSyncLock(listData);
                    try
                    {
                        if (listData.Count > 0)
                        {
                            foreach (SubscribeListResult item in listData.Values)
                            {
                                item.ResetData();
                                SubscribeToList(item.ListID);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    }
                    finally
                    {
                        omsCommon.ReleaseSyncLock(listData);
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (sockClient != null)
                {
                    sockClient.OnSocketMessage -= new EventHandler<SocketReceiveEventArgs>(HandleSocketMsg);
                    sockClient.OnError -= new EventHandler<SocketErrorEventArgs>(HandleError);
                    sockClient.OnSocketStatus -= new EventHandler<SocketStatusEventArgs>(HandleStatus);
                    sockClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion
    }
}