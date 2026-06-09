using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains P/Invoke declarations for dwmapi.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        #region P/Invoke — dwmapi.dll

        [DllImport("dwmapi.dll")]
        public static extern int DwmFlush();

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        #endregion
    }
}
