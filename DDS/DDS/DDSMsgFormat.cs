using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DDS
{
    public class DDSMsgFormat
    {
        public List<string> MainMode;
        public List<string> RepeatMode;
        Dictionary<string, TMessageFormat> formatList;
        
        string TStringObject;

        public DDSMsgFormat()
        {
            InitializeMsgFormat();
        }

        private void InitializeMsgFormat()
        {
            formatList = new Dictionary<string, TMessageFormat>();
            string key = string.Empty;
            TMessageFormat aMsg = new TMessageFormat();

            key = "CTR|image";
            aMsg.cammand = "image";
            aMsg.setting = "symbol|[attribute|value]";
            DDSMsgUtility.MyPrepareLayout(aMsg);
            formatList.Add(key, aMsg);
            
        }

        public static bool PrepareMessage(string iMessage, List<string> iMain, string iRepeat, out Dictionary<string, string> oRcvMessage)
        {
            oRcvMessage = new Dictionary<string, string>();
            return false;
        }

        public static bool PrepareLayout(string iLabel, string iMessage)
        {
            return true;
        }

    }

    class TMessageFormat
    {
        public string cammand;
        public string setting;
        public List<string> MainM;
        public List<string> RepeatM;
    }

    //class TStringObject
    //{
    //    public string str;
    //    public List<string> formatList;

    //    public TStringObject(string str)
    //    {
    //        this.str = str;
    //    }
    //}


    class DDSMsgUtility
    {
        public static List<string> MainMode, RepeatMode;

        public static void MyPrepareLayout(TMessageFormat aMsg)
        {
            string tSetting;
            int i, j;

        }

        

        public static void MyProcedureLayout(TMessageFormat aMsg)
        {
            string tSetting;
            int i, j;
        }

        public static bool CheckMsgFormat(string section, string dent)
        {
            int i;
            string key;
            TMessageFormat aMsg;

            bool CheckResult = false;
            key = section + "|" + dent;
            //i = 

            return true;
        }

        //public static bool PrepareMessage(String iMessage, String[] iMain, String[] iRepeat, List<string> oRcvMessage) //const String iMessage   
        //{
        //    return true;
        //}

        //public void InitializeMsgFormat()
        //{
        //    TMessageFormat aMsg;

        //}


    }


}
