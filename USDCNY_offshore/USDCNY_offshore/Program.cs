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
using System.Timers;

namespace USDCNY_offshore
{
    class Program
    {
        static void Main(string[] args)
        {


            Timer timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 30000;
            timer.Start();
            timer.Elapsed += timer_Elapsed;
            Console.Read();
            
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int hour = e.SignalTime.Hour;
            int minute = e.SignalTime.Minute;
            int second = e.SignalTime.Second;

            int sethour = 9;
            int setminute = 59;
            //int setseconde = 0;

            int maxhour = 12;
            int maxminute = 26;


            if (hour >= sethour )
            {
                GetWebPrice getter = new GetWebPrice();
                getter.GetTiemAndPrice();
                //getter.ConsolePrint();
                getter.LogTimeAndPrice();                
            }

            //if (hour == maxhour )
            //{
            //    System.Environment.Exit(0);
            //}
        }
    }
}
