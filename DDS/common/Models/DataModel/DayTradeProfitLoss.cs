using System;
using System.Collections.Generic;
using System.Text;
using OMS.common.Models.PositionModel;

namespace OMS.common.Models.DataModel
{
    public class DayTradeProfitLoss : ProfitLossBase
    {
        public DayTradeProfitLoss(PositionData data)
            : base(data)
        { }

        protected override void CalcBodPL()
        {
            totalQuantity = bodQty;
            totalAmount = posData.Price * bodQty;
            if (totalQuantity == 0) costPrice = 0m;
            else costPrice = totalAmount / totalQuantity;

            if (null != posData.Ticker && !posData.Ticker.SubItem.IsValid)
                posData.Ticker.SubItem.OnValidationChanged += new EventHandler(SubItem_OnValidationChanged);
            bodPL = bodQty * (GetPrice() - posData.PreClose) * posData.ContractSize;
        }

        protected override void SubItem_OnValidationChanged(object sender, EventArgs e)
        {
            if (omsCommon.SyncInvoker == null)
                System.Threading.Monitor.Enter(this);
            try
            {
                if (null!=posData.Ticker&& posData.Ticker.SubItem.IsValid)
                {
                    bodPL = bodQty * (GetPrice() - posData.PreClose) * posData.ContractSize;
                }
            }
            finally
            {
                if (omsCommon.SyncInvoker == null)
                    System.Threading.Monitor.Exit(this);
            }
        }
    }
}
