using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains P/Invoke declarations for kernel32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        #region P/Invoke — kernel32.dll

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        // CoInitializeEx / CoUninitialize are COM APIs exported by ole32.dll,
        // NOT kernel32.dll. Declaring the wrong module raised
        // EntryPointNotFoundException at runtime when icon extraction ran on its
        // own STA thread.
        [DllImport("ole32.dll")]
        public static extern IntPtr CoInitializeEx(IntPtr pvReserved, int dwCoInit);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        #endregion
    }
}
