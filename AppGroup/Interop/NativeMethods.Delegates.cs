using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains all delegate definitions used in Windows API callbacks.
    /// </summary>
    public static partial class NativeMethods
    {
        #region Delegates

        public delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        public delegate IntPtr SubclassProc(
            IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam,
            IntPtr uIdSubclass, IntPtr dwRefData);

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}
