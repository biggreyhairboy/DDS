using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using System.Text.RegularExpressions;

namespace OMS.common.Models.DataModel
{
    public class iOrderModel : OrderModelBase
    {
        public iOrderModel()
            : base()
        { }

        public void ProcessMessage(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            try
            {
                int startIndex = msg.IndexOf('|') + 1;

                if (msg.StartsWith("image|ORD_") || msg.StartsWith("image|ORDA_"))
                {
                    string orderStr = msg.Substring(startIndex).Trim();
                    if ("" != orderStr)
                    {
                        omsOrder order = new omsOrder(orderStr);
                        AddOrder(order);
                    }
                }
                else if (msg.StartsWith("image|SUBORDER_") || msg.StartsWith("querysuborder|"))
                {
                    string subOrderStr = msg.Substring(startIndex).Trim();
                    if ("" != subOrderStr)
                    {
                        amsOrder subOrder = new amsOrder(subOrderStr);
                        AddSuborder(subOrder);
                        if (msg.StartsWith("querysuboder|"))
                        {
                            Match m = Regex.Match(msg, @"^querysuborder\|(?<current>.*?)/(?<total>.*?)\|.*$");
                            if (m.Success)
                            {
                                string current = m.Groups["current"].Value;
                                string total = m.Groups["total"].Value;
                                if (current == total)
                                {
                                    QuerySuborderDone(subOrder);
                                }
                            }
                        }
                    }
                }
                else if (msg.StartsWith("image|TRADE_") || msg.StartsWith("querytrade|"))
                {
                    string tradeStr = msg.Substring(startIndex).Trim();
                    if ("" != tradeStr)
                    {
                        amsTrade trade = new amsTrade(tradeStr);
                        AddTrade(trade);
                        if (msg.StartsWith("querytrade|"))
                        {
                            Match m = Regex.Match(msg, @"^querytrade\|(?<current>.*?)/(?<total>.*?)\|.*$");
                            if (m.Success)
                            {
                                string current = m.Groups["current"].Value;
                                string total = m.Groups["total"].Value;
                                if (current == total)
                                {
                                    QueryTradeDone(trade);
                                }
                            }
                        }
                    }
                }
                else if (msg.StartsWith("image|ERRORD_"))
                {
                    string errOrderStr = msg.Substring(startIndex).Trim();
                    if (errOrderStr != "")
                    {
                        omsOrder errOrder = new omsOrder(errOrderStr);
                        AddErrorOrder(errOrder);
                    }
                }
                else if (msg.StartsWith("image|CALCD_"))
                {
                    AddCalculateMessage(msg.Substring(startIndex));
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }
    }
}
