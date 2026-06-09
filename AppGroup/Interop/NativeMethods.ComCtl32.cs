using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains P/Invoke declarations for comctl32.dll (Subclassing).
    /// </summary>
    public static partial class NativeMethods
    {
        #region P/Invoke — comctl32.dll (Subclassing)

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(
            IntPtr hWnd, SubclassProc pfnSubclass,
            IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool RemoveWindowSubclass(
            IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern IntPtr DefSubclassProc(
            IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}
