using System;
using System.Runtime.InteropServices;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains all constant values used in Windows API calls.
    /// </summary>
    public static partial class NativeMethods
    {
        #region Window Messages & Command IDs

        public const int WM_USER = 0x0400;
        public const int WM_COPYDATA = 0x004A;
        public const int WM_SETICON = 0x0080;
        public const uint WM_SHOWWINDOW = 0x0018;
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_DESTROY = 0x0002;
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_LBUTTONUP = 0x0202;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_NULL = 0x0000;
        public const uint WM_TRAYICON = 0x8000;

        public const int ID_SHOW = 1001;
        public const int ID_EXIT = 1002;
        public const int APPGROUP_SHOW_MAIN = 0x1;

        #endregion

        #region ShowWindow / SetWindowPos Flags

        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_MINIMIZE = 6;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWNOACTIVATE = 4;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;

        #endregion

        #region Window Style Flags

        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_WNDPROC = -4;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;

        #endregion

        #region Monitor / DPI Constants

        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        public const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint SPI_GETWORKAREA = 0x0030;
        public const int MDT_EFFECTIVE_DPI = 0;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        #endregion

        #region DWM Constants

        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        #endregion

        #region Layered Window

        public const int LWA_ALPHA = 0x00000002;

        #endregion

        #region Notify Icon (Tray)

        public const uint NIF_MESSAGE = 0x00000001;
        public const uint NIF_ICON = 0x00000002;
        public const uint NIF_TIP = 0x00000004;
        public const uint NIM_ADD = 0x00000000;
        public const uint NIM_MODIFY = 0x00000001;
        public const uint NIM_DELETE = 0x00000002;

        #endregion

        #region Shell / SHChangeNotify

        public const int SHCNE_RENAMEITEM = 0x00000001;
        public const int SHCNE_CREATE = 0x00000002;
        public const int SHCNE_DELETE = 0x00000004;
        public const int SHCNE_UPDATEIMAGE = 0x00008000;
        public const int SHCNE_UPDATEDIR = 0x00001000;
        public const int SHCNE_RENAMEFOLDER = 0x00020000;
        public const uint SHCNE_ASSOCCHANGED = 0x08000000;
        public const uint SHCNF_PATH = 0x0005;
        public const uint SHCNF_IDLIST = 0x0000;
        public const uint SHCNF_FLUSH = 0x1000;

        #endregion

        #region AppBar (Taskbar)

        public const uint ABM_GETSTATE = 0x4;
        public const uint ABM_GETTASKBARPOS = 0x5;
        public const int ABE_LEFT = 0;
        public const int ABE_TOP = 1;
        public const int ABE_RIGHT = 2;
        public const int ABE_BOTTOM = 3;

        #endregion

        #region Shell Icon

        public const int COINIT_APARTMENTTHREADED = 0x2;
        public const uint IMAGE_ICON = 1;
        public const uint LR_LOADFROMFILE = 0x00000010;
        public const uint LR_DEFAULTSIZE = 0x00000040;
        public const uint TPM_RETURNCMD = 0x0100;
        public const uint TPM_RIGHTBUTTON = 0x0002;
        public const uint MIN_ALL = 419;
        public const uint RESTORE_ALL = 416;
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        public const uint SHGFI_SYSICONINDEX = 0x4000;
        public const int SHIL_JUMBO = 0x4;

        #endregion

        #region RedrawWindow Flags

        public const uint RDW_ERASE = 0x0004;
        public const uint RDW_FRAME = 0x0400;
        public const uint RDW_INVALIDATE = 0x0001;
        public const uint RDW_ALLCHILDREN = 0x0080;

        #endregion

        #region WinEvent Hook

        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        #endregion

        #region GetWindow

        public const uint GW_OWNER = 4;

        #endregion

        #region Shell Icon Extraction

        public const uint SHGFI_POVERLAYINDEX = 0x00000020;
        public const uint SHGFI_PIDL = 0x000000008;

        #endregion

        #region AnimateWindow

        public const int AW_BLEND = 0x00080000;
        public const int AW_ACTIVATE = 0x00020000;

        #endregion

        #region HWND Constants

        public static readonly IntPtr HWND_TOP = IntPtr.Zero;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        #endregion

        #region Hook Constants

        public const int WH_MOUSE_LL = 14;
        public const int WM_LBUTTONDOWN = 0x0201;

        #endregion
    }
}
