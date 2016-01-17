using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace OMS.common.Utilities
{
    public class OmsHelper
    {
        public static string GetProductType(int prodType)
        {
            switch (prodType)
            {
                case OmsOrdConst.omsProductStock: return "Stock";
                case OmsOrdConst.omsProductFuture: return "Futures";
                case OmsOrdConst.omsProductOption: return "Options";
                case OmsOrdConst.omsProductWarrant: return "Warrant";
                case OmsOrdConst.omsProductBasketWarrant: return "BasketWarrant";
                case OmsOrdConst.omsProductBond: return "Bond";
                case OmsOrdConst.omsProductTrust: return "Trust";
                case OmsOrdConst.omsProductCurrency: return "Currency";
                case OmsOrdConst.omsProductStockIndex: return "Index";
                case OmsOrdConst.omsProductFund: return "Fund";
                default: return "N/A";
            }
        }

        public static int GetOrderStatus(string status)
        {
            if (status != null)
            {
                if (status.Length > 3)
                {
                    status = status.Substring(0, 4);
                }
                if (status == "Reje") return omsConst.omsOrderReject;
                else if (status == "Pend") return omsConst.omsOrderPending;
                else if (status == "Part") return omsConst.omsOrderPartialFill;
                else if (status == "Comp") return omsConst.omsOrderFill;
                else if (status == "Canc") return omsConst.omsOrderCancel;
                else if (status == "Inac") return omsConst.omsOrderInactive;
                else if (status == "Conf") return omsConst.omsOrderConfirm;
                else if (status == "Queu") return omsConst.omsOrderPending;
            }
            return omsConst.omsOrderNull;
        }

        //public static string GetOrderStatus(int status)
        //{
        //    switch (status)
        //    {
        //        case omsConst.omsOrderReject: return "Reje";
        //        case omsConst.omsOrderPending: return "Pend";
        //        case omsConst.omsOrderPartialFill: return "Part";
        //        case omsConst.omsOrderFill: return "Comp";
        //        case omsConst.omsOrderCancel: return "Canc";
        //        case omsConst.omsOrderInactive: return "Inac";
        //        case omsConst.omsOrderConfirm: return "Conf";
        //        default: return "";
        //    }
        //}

        public static string GetBuySell(int bsi)
        {
            switch (bsi)
            {
                case omsConst.omsOrderBuy: return "B";
                case omsConst.omsOrderSell: return "S";
                default: return " ";
            }
        }

        public static string GetDateTimeFromRow(DataRow row, string key, string format)
        {
            try
            {
                if (row[key] == null) return "";
                if (row[key] == DBNull.Value) return "";
                string res = row[key].ToString().Trim();
                if (res == "") return "";
                return DateTime.Parse(res).ToString(format);
            }
            catch { }
            return "";
        }

        public static string GetStringFromRow(DataRow row, string key)
        {
            try
            {
                if (row[key] == null) return "";
                if (row[key] == DBNull.Value) return "";
                return row[key].ToString().Trim();
            }
            catch { }
            return "";
        }

        public static int GetIntFromRow(DataRow row, string key)
        {
            try
            {
                if (row[key] == null) return 0;
                if (row[key] == DBNull.Value) return 0;
                string res = row[key].ToString().Trim();
                if (res == "") return 0;
                return int.Parse(res);
            }
            catch { }
            return 0;
        }

        public static decimal GetDecimalFromRow(DataRow row, string key)
        {
            try
            {
                if (row[key] == null) return 0m;
                if (row[key] == DBNull.Value) return 0m;
                string res = row[key].ToString().Trim();
                if (res == "") return 0m;
                return decimal.Parse(res);
            }
            catch { }
            return 0m;
        }

        public static bool GetBooleanFromRow(DataRow row, string key)
        {
            try
            {
                if (row[key] == null) return false;
                if (row[key] == DBNull.Value) return false;
                return (bool)row[key];
            }
            catch { }
            return false;
        }

        public static string GetDDSMode(DDSMode mode)
        {
            switch (mode)
            {
                case DDSMode.dmImage: return "image";
                case DDSMode.dmUpdate: return "update";
                default: return "both";
            }
        }

        public static bool IsLeadingByte(byte b)
        {
            if (b > 175 && b < 248) return true;//GB2312 leading byte range
            if (b > 128 && b < 255) return true;//BIG5 leading byte range
            return false;
        }

        public static bool CheckEncodingRange(byte[] bytes, ref string encoding)
        {
            int length = 0, i = 0, byteInt1 = 0, byteInt2 = 0;
            int BIGcount = 0, GBcount = 0;
            string name = "";

            if (bytes != null)
            {
                length = bytes.Length;
            }

            #region "check encoding range"
            while (i < length)
            {
                byteInt1 = bytes[i];
                if (byteInt1 < Convert.ToInt32("A1", 16))
                {
                    if (byteInt1 >= Convert.ToInt32("81", 16))
                    {
                        BIGcount++;
                    }
                    i++;
                    continue;
                }
                if (byteInt1 > Convert.ToInt32("F7", 16))
                {
                    if (byteInt1 <= Convert.ToInt32("F9", 16))
                        BIGcount++;
                    i++;
                    continue;
                }
                if (byteInt1 >= Convert.ToInt32("A1", 16) && byteInt1 <= Convert.ToInt32("F7", 16))
                {
                    GBcount++;
                    if (i + 1 < length)
                    {
                        if (byteInt1 != Convert.ToInt32("C8", 16) && byteInt1 != Convert.ToInt32("C7", 16) && !(byteInt1 == Convert.ToInt32("C6", 16) && byteInt2 >= Convert.ToInt32("7F", 16) && byteInt2 <= Convert.ToInt32("FF", 16)))
                            BIGcount++;
                    }
                    i += 2;
                }
                else
                {
                    i++;
                }
            }

            #endregion

            #region "check string"
            if (BIGcount > GBcount)
            {
                name = System.Text.Encoding.GetEncoding("BIG5").GetString(bytes);
                if (CheckString(bytes, name))
                {
                    encoding = "BIG5";
                    return true;
                }
                return false;
            }
            else if (BIGcount < GBcount)
            {
                name = System.Text.Encoding.GetEncoding("GB2312").GetString(bytes);
                if (CheckString(bytes, name))
                {
                    encoding = "GB2312";
                    return true;
                }
                return false;
            }
            else
                return false;

            #endregion
        }

        private static bool CheckString(byte[] bytes, string name)
        {
            int i = 0, stringloop = 0, length = bytes.Length, strlen = name.Length;
            byte byteInt1;
            while (i < length)
            {
                byteInt1 = bytes[i];
                if (byteInt1 <= 127)
                {
                    if (byteInt1 == 63)
                    {
                        if (strlen > stringloop && name[stringloop] == '?')
                        {
                            i += 1;
                            stringloop += 1;
                            continue;
                        }
                        else
                            return false;
                    }
                    i += 1;
                    stringloop += 1;
                    continue;
                }
                else
                {
                    i += 2;
                    stringloop += 1;
                }
            }
            return true;
        }

        public static void FisherYates(ref char[] arr)
        {
            int i = arr.Length;
            if (i == 0) return;
            Random rdm = new Random(Guid.NewGuid().GetHashCode());
            int j = 0;
            char tmp;
            while (--i > -1)
            {
                j = rdm.Next() % (i + 1);
                tmp = arr[i];
                arr[i] = arr[j];
                arr[j] = tmp;
            }
        }

        public static string GetRandomStr(string str)
        {
            int i = str.Length;
            if (i == 0) return str;
            char[] ls = str.ToCharArray();
            FisherYates(ref ls);
            StringBuilder trStr=new StringBuilder();
            trStr.Append(ls);
            return trStr.ToString();
        }
    }
}
