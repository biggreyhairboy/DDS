using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace OMS.common.Utilities
{
    internal class User32
    {
        public const int SM_DBCSENABLED = 42;

        [DllImport("user32")]
        public static extern int GetSystemMetrics(int nIndex);
    }
}
