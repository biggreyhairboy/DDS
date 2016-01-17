using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace OMS.common.Utilities
{
    public class Crypt32
    {
        public const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        public const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

        [StructLayout(LayoutKind.Sequential)]
        public struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("crypt32", CharSet = CharSet.Auto)]
        public static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);

        [DllImport("crypt32", CharSet = CharSet.Auto)]
        public static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, StringBuilder szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);
    }
}
