using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OMS.common.Utilities
{
    public class CSVParser
    {
        private static CSVParser instance = new CSVParser();
        private string pattern = @"(?:^|,)(?:""(?<value>(?>[^""]+|"""")*)""|(?<value>[^"",]*))";
        private Regex regex;
        private bool trimWhitespace;

        private CSVParser()
        {
            regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            trimWhitespace = false;
        }

        public bool TrimWhitespace { get { return trimWhitespace; } set { trimWhitespace = value; } }

        public List<string> Split(string msg)
        {
            if (msg == null || msg.Trim() == "") return null;
            List<string> buff = new List<string>();

            for (Match m = regex.Match(msg); m.Success; m = m.NextMatch())
            {
                if (trimWhitespace) buff.Add(m.Groups["value"].Value.Trim());
                else buff.Add(m.Groups["value"].Value);
            }

            return buff;
        }

        public static CSVParser Instance { get { return instance; } }
    }
}
