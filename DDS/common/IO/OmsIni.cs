using System;
using System.IO;
using System.Collections.Generic;

namespace OMS.common.IO
{
    public class OmsIni
    {
        public const string OmsIniDefaultFileName = "oms.ini";

        private static volatile object syncRoot = new object();
        private static OmsIni instance;

        protected Dictionary<string, IniBlock> settings;

        private OmsIni() { }

        public string this[string section, string key]
        {
            get
            {
                try
                {
                    if (OmsIniFile.ContainsKey(section))
                    {
                        IniBlock block = OmsIniFile[section] as IniBlock;
                        if (block.ContainsKey(key))
                            return block[key];
                    }
                }
                catch { }
                return "";
            }
        }

        public Dictionary<string, IniBlock> OmsIniFile
        {
            get
            {
                if (settings == null)
                {
                    lock (syncRoot)
                    {
                        if (settings == null)
                        {
                            string fileName = OmsIniDefaultFileName;
                            if (!File.Exists(fileName))
                            {
                                fileName = Path.Combine(Environment.GetEnvironmentVariable("windir"), OmsIniDefaultFileName);
                            }
                            if (!File.Exists(fileName))
                            {
                                throw new FileNotFoundException("Cannot find ini settings");
                            }
                            IniReader reader = new IniReader(fileName);
                            settings = reader.GetIniBlock();
                        }
                    }
                }
                return settings;
            }
        }

        public static OmsIni Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new OmsIni();
                        }
                    }
                }
                return instance;
            }
        }
    }
}