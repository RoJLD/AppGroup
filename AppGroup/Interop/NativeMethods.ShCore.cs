using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains P/Invoke declarations for Shcore.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        #region P/Invoke — Shcore.dll

        // Returns HRESULT; 0 (S_OK) = success
        [DllImport("Shcore.dll")]
        public static extern int GetDpiForMonitor(
            IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        #endregion
    }
}
