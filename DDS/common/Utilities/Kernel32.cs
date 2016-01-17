using System;
using System.Text;
using System.Runtime.InteropServices;

namespace OMS.common.Utilities
{
    internal class Kernel32
    {
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileSection(string section, byte[] val, int size, string filepath);

        [DllImport("kernel32")]
        public static extern bool IsDBCSLeadByte(byte testChar);

        [DllImport("kernel32")]
        public static extern IntPtr LocalFree(IntPtr hMem);
    }
}
