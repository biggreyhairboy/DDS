using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Utilities;
using OMS.common.Models.PositionModel;

namespace OMS.common.Models.DataModel
{
    public class ProfitLossBase
    {
        protected PositionData posData;
        protected SortedDictionary<string, amsTrade> innerTimeTrades;//Key: "<TradeTime>=<TradeNum>"
        protected decimal bodQty;
        protected decimal realPL;
        protected decimal unrealPL;
        protected decimal bodPL;
        protected decimal exePL;
        protected decimal totalQuantity;
        protected decimal totalAmount;
        protected decimal costPrice;
        protected bool optionPLPrice;

        protected decimal avgPrice;
        protected decimal avgTotalQty;
        protected decimal avgTotalAmt;

        public ProfitLossBase(PositionData posData)
        {
            this.posData = posData;
            innerTimeTrades = new SortedDictionary<string, amsTrade>();
        }

        public SortedDictionary<string, amsTrade>.ValueCollection.Enumerator InnerTimerTradesObj { get { return innerTimeTrades.Values.GetEnumerator(); } }

        public bool OptionPLPrice { get { return optionPLPrice; } set { optionPLPrice = value; } }
        
        public decimal AvgPrice
        {
            get
            {
                switch (omsCommon.AveragePriceMode)
                {
                    case OmsAvgPriceMode.apmFIFN: return costPrice;
                    case OmsAvgPriceMode.apmASHARE: return avgPrice;
                }
                return costPrice;
            }
        }

        public virtual void InitBodQuantity(decimal bodQty)
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(this);
            try
            {
                this.bodQty = bodQty;

                this.avgTotalQty = bodQty;
                avgTotalAmt = bodQty * posData.BodAvgPrice;
                if (avgTotalQty == 0) avgPrice = 0m;
                else avgPrice = avgTotalAmt / avgTotalQty;

                CalcBodPL();
                //unrealPL = exePL + bodPL - realPL;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(this);
            }
        }

        protected virtual void CalcBodPL()
        {
            totalQuantity = bodQty;
            totalAmount = posData.Price * bodQty;
            if (totalQuantity == 0) costPrice = 0m;
            else costPrice = totalAmount / totalQuantity;

            if (null != posData.Ticker && !posData.Ticker.SubItem.IsValid)
                posData.Ticker.SubItem.OnValidationChanged += new EventHandler(SubItem_OnValidationChanged);
            bodPL = bodQty * (GetPrice() - posData.Price) * posData.ContractSize;
        }

        internal virtual void Reset()
        {
            bodPL = 0m;
            exePL = 0m;
            realPL = 0m;
            unrealPL = 0m;
            CalcBodPL();
        }

        internal virtual void Recalc()
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(this);
            try
            {
                Reset();
                foreach (amsTrade trade in innerTimeTrades.Values)
                {
                    CalcPL(trade);
                }
                //unrealPL = exePL + bodPL - realPL;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(this);
            }
        }

        protected virtual void SubItem_OnValidationChanged(object sender, EventArgs e)
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(this);
            try
            {
                if (null != posData.Ticker && posData.Ticker.SubItem.IsValid)
                {
                    // bodPL = bodQty * (GetPrice() - posData.Price) * posData.ContractSize;
                    // bodPL = CurrencyProcessor.Instance.InBaseCurrency(posData.Currency, bodPL);
                    Recalc();
                    //If Price is ready, we need recalculate the PNL
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(this);
            }
        }

        public decimal RealizedPL { get { return realPL; } }

        public decimal UnrealizedPL 
        {
            get
            {
                if (totalQuantity == 0) costPrice = 0m;
                else costPrice = totalAmount / totalQuantity;
                unrealPL = (GetPrice() - costPrice) * totalQuantity * posData.ContractSize;
                return unrealPL;
            } 
        }

        public decimal BodPL { get { return bodPL; } }

        public decimal ExecutePL 
        {
            get
            {
                int bsi = 1;
                decimal tmpExePL;
                exePL = 0;
                decimal tradePrice;
                decimal CurrPrice = GetPrice();
                foreach (amsTrade trade in innerTimeTrades.Values)
                {
                    tradePrice = trade.Price;
                    if (posData.SIOperatorFlag.IndexOf(trade.OperatorFlag) >= 0 && null != posData.Ticker)
                    {
                        tradePrice = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
                    }
                    if (trade.OrderType == omsConst.omsOrderSell) bsi = -1;
                    else bsi = 1;
                    tmpExePL = trade.Quantity * (CurrPrice - tradePrice) * posData.ContractSize * bsi;
                    exePL += tmpExePL;
                }
                return exePL;
            } 
        }

        internal void AddTrade(amsTrade trade)
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(this);
            try
            {
                if (trade == null) return;
                //when give cash to account,not to calc unrealized pl
                if ((trade.Price != 0) && (CurrencyProcessor.Instance.IsCashSymbol(trade.Symbol)))
                { return; }
                //
                string key = trade.Time + "=" + trade.TradeNum;
                amsTrade oldTrade = trade.Data as amsTrade;
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Enter(innerTimeTrades);
                try
                {
                    if (oldTrade != null)
                    {
                        string oldKey = oldTrade.Time + "=" + oldTrade.TradeNum;
                        if (innerTimeTrades.ContainsKey(oldKey))
                            innerTimeTrades.Remove(oldKey);
                    }
                    if(trade.Status == omsConst.omsOrderFill)
                        innerTimeTrades[key] = trade;
                }
                finally
                {
                    if (omsCommon.SyncInvoker == null)
                        System.Threading.Monitor.Exit(innerTimeTrades);
                }
                if (oldTrade != null)
                {
                    decimal oldTradePrice = oldTrade.Price;
                    if (posData.SIOperatorFlag.IndexOf(oldTrade.OperatorFlag) >= 0 && null != posData.Ticker)
                    {
                        oldTradePrice = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
                        if (oldTradePrice == 0) oldTradePrice = oldTrade.PreClose;//temp solution
                    }
                    if (oldTrade.OrderType == omsConst.omsOrderBuy)
                    {
                        avgTotalQty -= oldTrade.Quantity;
                        avgTotalAmt -= oldTrade.Quantity * oldTradePrice;
                    }
                    else if (oldTrade.OrderType == omsConst.omsOrderSell)
                    {
                        avgTotalQty += oldTrade.Quantity;
                        avgTotalAmt += oldTradePrice * oldTrade.Quantity;
                    }
                }
                decimal tradePrice = trade.Price;
                if (posData.SIOperatorFlag.IndexOf(trade.OperatorFlag) >= 0 && null != posData.Ticker)
                {
                    tradePrice = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
                    if (tradePrice == 0) tradePrice = trade.PreClose;//temp solution
                }
                if (trade.OrderType == omsConst.omsOrderSell && trade.Status == omsConst.omsOrderFill)
                {
                    avgTotalQty -= trade.Quantity;
                    avgTotalAmt -= trade.Quantity * tradePrice;
                }
                else if (trade.OrderType == omsConst.omsOrderBuy && trade.Status == omsConst.omsOrderFill)
                {
                    avgTotalQty += trade.Quantity;
                    avgTotalAmt += trade.Quantity * tradePrice;
                }
                if (avgTotalQty == 0) avgPrice = 0m;
                else avgPrice = avgTotalAmt / avgTotalQty;

                if (oldTrade != null)
                {
                    Recalc();
                    return;
                }
                CalcPL(trade);
                //unrealPL = exePL + bodPL - realPL;
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(this);
            }
        }

        protected void CalcPL(amsTrade trade)
        {
            if (trade == null) return;
            decimal tradePrice = trade.Price;
            if (posData.SIOperatorFlag.IndexOf(trade.OperatorFlag) >= 0 && null != posData.Ticker)
            {
                tradePrice = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_PREV_CLOSE);
            }
            if (((trade.OrderType == omsConst.omsOrderSell) && (totalQuantity > 0)) || ((trade.OrderType == omsConst.omsOrderBuy) && (totalQuantity < 0)))
            {
                int flag = 1;
                if (totalQuantity < 0) flag = -1;
                decimal netQty = Math.Min(Math.Abs(totalQuantity), Math.Abs(trade.Quantity));
                decimal tmpRealPL = 0m;
                tmpRealPL = netQty * (tradePrice - costPrice) * posData.ContractSize * flag;
                TLog.DefaultInstance.WriteDebugLog(string.Format("TradeNo: {0}, CurrentQty: {1}, TradeQty: {2}, NetQty: {3}, TradePrice: {4}, CostPrice: {5}, Flag: {6}", trade.TradeNum, totalQuantity, trade.Quantity, netQty, tradePrice, costPrice, flag));
                realPL += tmpRealPL;
            }
          /*  int bsi = 1;
            if (trade.OrderType == omsConst.omsOrderSell) bsi = -1;
            decimal tmpExePL = trade.Quantity * (GetPrice() - tradePrice) * posData.ContractSize * bsi;
            tmpExePL = CurrencyProcessor.Instance.InBaseCurrency(posData.Currency, tmpExePL);
            exePL += tmpExePL;
        */

            if (trade.OrderType == omsConst.omsOrderBuy)
            {
                if (omsCommon.PAndLMode == OmsAvgPriceMode.apmFIFN)
                {
                    if (totalQuantity >= 0)
                    {
                        totalQuantity += trade.Quantity;
                        totalAmount += tradePrice * trade.Quantity;
                    }
                    else
                    {
                        decimal netQty = Math.Min(Math.Abs(totalQuantity), Math.Abs(trade.Quantity));
                        totalQuantity += trade.Quantity;
                        totalAmount += costPrice * netQty;
                        decimal leftQty = Math.Abs(Math.Abs(trade.Quantity) - netQty);
                        if (leftQty > 0)
                        {
                            totalAmount += leftQty * tradePrice;
                        }
                    }
                }
                else
                {
                    totalQuantity += trade.Quantity;
                    totalAmount += tradePrice * trade.Quantity;
                }
            }
            else if (trade.OrderType == omsConst.omsOrderSell)
            {
                if (omsCommon.PAndLMode == OmsAvgPriceMode.apmFIFN)
                {
                    if (totalQuantity > 0)
                    {
                        decimal netQty = Math.Min(Math.Abs(totalQuantity), Math.Abs(trade.Quantity));
                        totalQuantity -= trade.Quantity;
                        totalAmount -= costPrice * netQty;
                        decimal leftQty = Math.Abs(Math.Abs(trade.Quantity) - netQty);
                        if (leftQty > 0)
                        {
                            totalAmount -= leftQty * tradePrice;
                        }
                    }
                    else
                    {
                        totalQuantity -= trade.Quantity;
                        totalAmount -= tradePrice * trade.Quantity;
                    }
                }
                else
                {
                    totalQuantity -= trade.Quantity;
                    totalAmount -= tradePrice * trade.Quantity;
                }
            }
            if (totalQuantity == 0) costPrice = 0m;
            else costPrice = totalAmount / totalQuantity;
            if(omsCommon.LogDebugInfo)
                TLog.DefaultInstance.WriteLog(string.Format("DEBUG CalcPL Account:{0},TradeNo:{1},TradeQty:{2},TradePrice:{3},TotalAmount:{4},TotalQty:{5},CostPrice:{6}", trade.Account, trade.TradeNum, trade.Quantity, tradePrice,totalAmount,totalQuantity,costPrice), LogType.INFO);
        }

        protected decimal GetPrice()
        {
            if (optionPLPrice)
            {
                if ((posData.ProductType == OmsOrdConst.omsProductOption) && IsMarketOpen() && null != posData.Ticker)
                {
                    if (posData.Quantity > 0)
                    {
                        decimal qty = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_BID_SIZE);
                        if (qty > 0)
                            return posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_BID);
                    }
                    else
                    {
                        decimal qty = posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_OFFER_SIZE);
                        if (qty > 0)
                            return posData.Ticker.SubItem.GetAttributeAsDecimal(omsConst.OMS_OFFER);
                    }
                }
            }
            return posData.Nominal;
        }

        protected bool IsMarketOpen()
        {
            if (omsCommon.OptionPLPriceCheckMarket)
            {
                return OmsMarketManager.Instance.IsMarketOpen(posData.Exchange);
            }
            return true;
        }
    }
}
