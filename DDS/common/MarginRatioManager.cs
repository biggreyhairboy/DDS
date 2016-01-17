using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Database;
using System.Threading;
using System.Data;
using OMS.common.Utilities;

namespace OMS.common
{
    public interface MarginRatioSubscriber
    {
        string Symbol { get;}
        void HandlerMarginRatioUpdate(decimal marginRatio);
    }

    public class MarginRatioManager
    {
        private static Dictionary<string,MarginRatioManager> instances = new Dictionary<string,MarginRatioManager>();
        private volatile static object syncRoot = new object();
        private Dictionary<string,decimal> marginRatios = null;
        private List<MarginRatioSubscriber> marginRatioSubscribers = null;
        public bool isExcuting = false;
        private AutoResetEvent eventSelectMarginRatio;
        protected ManualResetEvent stopEvent;
        private Queue<string> symbolQueue;
        private string databaseAlias;
        private MarginRatioManager(string dbAlias)
        {
            this.databaseAlias = dbAlias;
            marginRatios = new Dictionary<string,decimal>();
            marginRatioSubscribers = new List<MarginRatioSubscriber>();
            symbolQueue = new Queue<string>();
            eventSelectMarginRatio = new AutoResetEvent(false);
            stopEvent = new ManualResetEvent(false);
            Thread threadSelectMarginRatio = new Thread(new ThreadStart(SelectMarginRatio));
            threadSelectMarginRatio.Start();
        }
        public static MarginRatioManager GetInstance(string databaseAlias)
        {
            if (!instances.ContainsKey(databaseAlias))
            {
                lock (syncRoot)
                {
                    if (!instances.ContainsKey(databaseAlias))
                        instances[databaseAlias] = new MarginRatioManager(databaseAlias);
                }
            }
            return instances[databaseAlias];
        }

        public decimal GetMarginRatioOf(string symbol, MarginRatioSubscriber anSubscriber)
        {
            SubscribeManager.Instance.RequestVerify();
            if (symbol == null || symbol.Trim() == "") return 1m;
            lock (syncRoot)
            {
                if (marginRatios.ContainsKey(symbol))
                    return marginRatios[symbol];
                else
                {
                    if (!marginRatioSubscribers.Contains(anSubscriber))
                    {
                        marginRatioSubscribers.Add(anSubscriber);
                        symbolQueue.Enqueue(symbol);
                        eventSelectMarginRatio.Set();
                    }
                    return 1m;
                }
            }
        }

        private void SelectMarginRatio()
        {
            WaitHandle[] handles = new WaitHandle[] {stopEvent, eventSelectMarginRatio};
            Queue<string> workQueue = new Queue<string>();
            List<MarginRatioSubscriber> workSubscriber = new List<MarginRatioSubscriber>();
            while (true)
            {
                if (WaitHandle.WaitAny(handles) == 0)
                {
                    break;
                }
                else
                {
                    lock (workQueue)
                    {

                        lock (syncRoot)
                        {
                            if (symbolQueue.Count <= 0) continue;
                            workQueue.Clear();
                            foreach (string symbol in symbolQueue)
                            {
                                workQueue.Enqueue(symbol);
                            }
                            symbolQueue.Clear();

                            workSubscriber.Clear();
                            foreach (MarginRatioSubscriber anSubscriber in marginRatioSubscribers)
                            {
                                workSubscriber.Add(anSubscriber);
                            }
                        }
                        if (workQueue.Count <= 0) continue;
                        StringBuilder buffer = new StringBuilder();
                        foreach (string symbol in workQueue)
                        {
                            if (buffer.Length == 0) buffer.Append(string.Format("'{0}'", symbol));
                            else buffer.Append(string.Format(",'{0}'", symbol));
                        }
                        string sql = string.Format("select * from margin where symbol in ({0})", buffer);
                        DataSet ds = OmsDatabaseManager.Instance.GetDataAmbiguous(databaseAlias, sql);
                        if (ds == null || ds.Tables.Count <= 0) continue;
                        DataTable table = ds.Tables[0];
                        List<MarginRatioSubscriber> removableSubscriber = new List<MarginRatioSubscriber>();

                        foreach (DataRow row in table.Rows)
                        {
                            string symbol = OmsHelper.GetStringFromRow(row, "symbol");
                            decimal marginRatio = OmsHelper.GetDecimalFromRow(row, "margin");
                            if (!marginRatios.ContainsKey(symbol))
                            {
                                marginRatios.Add(symbol, marginRatio);
                                foreach (MarginRatioSubscriber anSubscriber in workSubscriber)
                                {
                                    if (anSubscriber.Symbol == symbol)
                                    {
                                        anSubscriber.HandlerMarginRatioUpdate(marginRatio);
                                        removableSubscriber.Add(anSubscriber);
                                    }
                                }
                            }
                        }

                        lock (syncRoot)
                        {
                            foreach (MarginRatioSubscriber anSubscriber in removableSubscriber)
                            {
                                if (marginRatioSubscribers.Contains(anSubscriber))
                                    marginRatioSubscribers.Remove(anSubscriber);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            stopEvent.Set();
        }
    }
}