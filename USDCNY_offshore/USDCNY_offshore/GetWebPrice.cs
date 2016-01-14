using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace USDCNY_offshore
{
    class GetWebPrice
    {
        public string time;
        public string buy;
        public string sell;

        public GetWebPrice()
        {
            this.time = string.Empty;
            this.buy = string.Empty ;
            this.sell = string.Empty;
        }

        public void GetTiemAndPrice()
        {
            string Url = "http://services1.aastocks.com/web/bchk/bochk/mktinfo.aspx?BCHKLanguage=eng&pagetype=8";
            //
            //view state 
            string postDataStr = "__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUKMTM4NzUxMDY2OGQYAgUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgQFGmN0bDAwJEhlYWRlcjEkYnRuQ3Jvc3NSYXRlBRZjdGwwMCRIZWFkZXIxJGJ0bkNoYXJ0BRVjdGwwMCRIZWFkZXIxJGJ0bk5ld3MFJmN0bDAwJENvbnRlbnQkdWNDcm9zc1JhdGUkaW1nQnRuU2VhcmNoBSVjdGwwMCRDb250ZW50JHVjQ3Jvc3NSYXRlJGd2Q3Jvc3NSYXRlDzwrAAoBCAIBZC9JBi4s67OoHRxfU8nCDgMAAAAA&__VIEWSTATEGENERATOR=5ECD26ED&__PREVIOUSPAGE=JWJVcbsFF20iv3DE8Eail0Oh5qLIb_hPwXBucUJ1WeVaZK5DtiKMAH2SKF7vdDNiFWBScZTvOPUAAAAA0&ctl00%24Content%24ucCrossRate%24ddlCrossRate=CNH&ctl00%24Content%24ucCrossRate%24imgBtnSearch.x=41&ctl00%24Content%24ucCrossRate%24imgBtnSearch.y=13";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            //request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();

            string[] strarr = retString.Split('\r');
            //string time = string.Empty;
            //string buy = string.Empty;
            //string sell = string.Empty;
            for (int i = 0; i < strarr.Length; i++)
            {
                if (strarr[i].Contains("<span id=\"ctl00_Content_lblUpdateTime\">Last updated :</span>&nbsp;"))
                {
                    time = strarr[i].Trim().Substring(66, 19);
                    continue;
                }
                else if (strarr[i].Contains("<span id=\"ctl00_Content_ucCrossRate_gvCrossRate_ctl15_Label1\">"))
                {
                    buy = strarr[i].Trim().Substring(62, 9);
                    continue;
                }
                else if (strarr[i].Contains("<span id=\"ctl00_Content_ucCrossRate_gvCrossRate_ctl15_Label2\">"))
                {
                    sell = strarr[i].Trim().Substring(62, 9);
                    break;
                }

            }
        }

        public void ConsolePrint()
        {
            Console.WriteLine("update time is: " + time);
            Console.WriteLine("buy price is: " + buy);
            Console.WriteLine("sell price is: " + sell);
            Console.ReadLine();
        }

        public void LogTimeAndPrice()
        {
            FileStream fs = new FileStream(@".\log.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("\r\n");
            sw.WriteLine("log time" + DateTime.Now.ToString(""));
            sw.WriteLine("update time is: " + time);
            sw.WriteLine("buy price is: " + buy);
            sw.WriteLine("sell price is: " + sell);
            sw.Close();
            fs.Close();

        }
    }
}
