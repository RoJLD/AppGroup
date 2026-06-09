using Microsoft.UI.Windowing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Graphics;

namespace AppGroup.Interop
{
    public static partial class NativeMethods
    {
        #region Static Helper Methods

        public static readonly int WM_UPDATE_GROUP = RegisterWindowMessage("AppGroup.WM_UPDATE_GROUP");

        public static IntPtr LoadIcon(string iconPath) =>
            LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_DEFAULTSIZE | LR_LOADFROMFILE);

        public static void SendString(IntPtr targetWindow, string message)
        {
            var cds = new COPYDATASTRUCT
            {
                dwData = (IntPtr)100,
                cbData = (message.Length + 1) * 2,
                lpData = Marshal.StringToHGlobalUni(message)
            };
            try
            {
                IntPtr cdsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cds));
                Marshal.StructureToPtr(cds, cdsPtr, false);
                SendMessage(targetWindow, (uint)WM_COPYDATA, IntPtr.Zero, cdsPtr);
                Marshal.FreeHGlobal(cdsPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(cds.lpData);
            }
        }

        public static void ForceForegroundWindow(IntPtr hWnd)
        {
            if (GetForegroundWindow() == hWnd) return;

            IntPtr foregroundWindow = GetForegroundWindow();
            uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, out _);
            uint currentThreadId = GetCurrentThreadId();

            if (currentThreadId != foregroundThreadId)
                AttachThreadInput(currentThreadId, foregroundThreadId, true);

            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
            SetFocus(hWnd);

            if (currentThreadId != foregroundThreadId)
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
        }

        public static float GetDpiScaleForMonitor(IntPtr hMonitor)
        {
            try
            {
                var v = Environment.OSVersion.Version;
                if (v.Major > 6 || (v.Major == 6 && v.Minor >= 3))
                {
                    if (GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0)
                        return dpiX / 96.0f;
                }
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    return g.DpiX / 96.0f;
            }
            catch { return 1.0f; }
        }

        public static bool IsTaskbarAutoHide()
        {
            var data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            IntPtr result = SHAppBarMessage(ABM_GETSTATE, ref data);
            return ((uint)result & 0x01) != 0;
        }

        #endregion

        #region Taskbar-Aware Window Positioning

        private enum TaskbarPosition { Top, Bottom, Left, Right }

        private static bool IsCursorOnTaskbar(POINT cursorPos, MONITORINFO mi, TaskbarPosition pos)
        {
            return pos switch
            {
                TaskbarPosition.Top => cursorPos.Y < mi.rcWork.top,
                TaskbarPosition.Bottom => cursorPos.Y >= mi.rcWork.bottom,
                TaskbarPosition.Left => cursorPos.X < mi.rcWork.left,
                TaskbarPosition.Right => cursorPos.X >= mi.rcWork.right,
                _ => false
            };
        }

        private static TaskbarPosition GetTaskbarPosition(MONITORINFO mi)
        {
            bool workEqualsMonitor =
                mi.rcWork.top == mi.rcMonitor.top &&
                mi.rcWork.bottom == mi.rcMonitor.bottom &&
                mi.rcWork.left == mi.rcMonitor.left &&
                mi.rcWork.right == mi.rcMonitor.right;

            if (workEqualsMonitor)
                return GetTaskbarPositionFromAppBarInfo();

            if (mi.rcWork.top > mi.rcMonitor.top) return TaskbarPosition.Top;
            if (mi.rcWork.bottom < mi.rcMonitor.bottom) return TaskbarPosition.Bottom;
            if (mi.rcWork.left > mi.rcMonitor.left) return TaskbarPosition.Left;
            if (mi.rcWork.right < mi.rcMonitor.right) return TaskbarPosition.Right;
            return TaskbarPosition.Bottom;
        }

        private static TaskbarPosition GetTaskbarPositionFromAppBarInfo()
        {
            var data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            if (SHAppBarMessage(ABM_GETTASKBARPOS, ref data) != IntPtr.Zero)
            {
                return data.uEdge switch
                {
                    (uint)ABE_TOP => TaskbarPosition.Top,
                    (uint)ABE_BOTTOM => TaskbarPosition.Bottom,
                    (uint)ABE_LEFT => TaskbarPosition.Left,
                    (uint)ABE_RIGHT => TaskbarPosition.Right,
                    _ => TaskbarPosition.Bottom
                };
            }
            return TaskbarPosition.Bottom;
        }

        public static void PositionWindowOffScreen(IntPtr hWnd)
        {
            int screenHeight = (int)DisplayArea.Primary.WorkArea.Height;
            
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | (uint)SWP_HIDEWINDOW);

            SetWindowPos(hWnd, IntPtr.Zero, 0, screenHeight + 100, 0, 0,
                SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public static void PositionWindowOffScreenBelow(IntPtr hWnd)
        {
            try
            {
                if (!GetCursorPos(out POINT cursor))
                {
                    cursor.X = GetSystemMetrics(SM_CXSCREEN) / 2;
                    cursor.Y = GetSystemMetrics(SM_CYSCREEN) / 2;
                }

                if (!GetWindowRect(hWnd, out RECT wr)) return;
                int windowWidth = wr.right - wr.left;
                int windowHeight = wr.bottom - wr.top;

                IntPtr primary = MonitorFromPoint(
                    new POINT { X = 0, Y = 0 }, (uint)MONITOR_DEFAULTTOPRIMARY);
                var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };

                if (!GetMonitorInfo(primary, ref mi))
                {
                    int sx = GetSystemMetrics(SM_CXSCREEN);
                    SetWindowPos(hWnd, IntPtr.Zero, sx + 100, cursor.Y, 0, 0,
                        SWP_NOSIZE | SWP_NOZORDER);
                    return;
                }

                int safetyMargin = Math.Max(windowWidth, 200);
                int offX = mi.rcMonitor.right + 100 + safetyMargin;
                int offY = cursor.Y;

                SetWindowPos(hWnd, IntPtr.Zero, offX, offY, 0, 0,
                    SWP_NOSIZE | SWP_NOZORDER);

                Debug.WriteLine(
                    $"PositionOffScreenBelow -> ({offX},{offY}), cursor at ({cursor.X},{cursor.Y})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PositionWindowOffScreenBelow error: {ex.Message}");
            }
        }

        public static void PositionWindowAboveTaskbar(IntPtr hWnd, bool show = true, POINT? cursorOverride = null)
        {
            try
            {
                if (!GetWindowRect(hWnd, out RECT wr)) return;
                int windowWidth = wr.right - wr.left;
                int windowHeight = wr.bottom - wr.top;

                POINT cursor;
                if (cursorOverride.HasValue)
                    cursor = cursorOverride.Value;
                else if (!GetCursorPos(out cursor))
                    return;

                IntPtr monitor = MonitorFromPoint(cursor, MONITOR_DEFAULTTONEAREST);
                var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
                if (!GetMonitorInfo(monitor, ref mi)) return;

                float dpiScale = GetDpiScaleForMonitor(monitor);
                int baseTaskbarH = 52;
                bool autoHide = IsTaskbarAutoHide();
                TaskbarPosition tbPos = GetTaskbarPosition(mi);

                int spacing;
                if (autoHide)
                {
                    spacing = IsCursorOnTaskbar(cursor, mi, tbPos)
                        ? (int)(baseTaskbarH * dpiScale)
                        : (int)(11 * dpiScale);
                }
                else
                {
                    spacing = tbPos == TaskbarPosition.Top
                        ? (int)(10 * dpiScale)
                        : (int)(6 * dpiScale);
                }

                int x = cursor.X - windowWidth / 2;
                int y;

                switch (tbPos)
                {
                    case TaskbarPosition.Top:
                    case TaskbarPosition.Bottom:
                        if (IsCursorOnTaskbar(cursor, mi, tbPos))
                        {
                            y = tbPos == TaskbarPosition.Top
                                ? mi.rcWork.top + spacing
                                : mi.rcWork.bottom - windowHeight - spacing;
                        }
                        else
                        {
                            y = cursor.Y - windowHeight - spacing;
                            if (y < mi.rcWork.top + spacing)
                                y = mi.rcWork.top + spacing;
                        }
                        break;
                    case TaskbarPosition.Left:
                        x = mi.rcWork.left + spacing;
                        y = cursor.Y - windowHeight / 2;
                        if (y < mi.rcWork.top + spacing)
                            y = mi.rcWork.top + spacing;
                        if (y + windowHeight > mi.rcWork.bottom - spacing)
                            y = mi.rcWork.bottom - windowHeight - spacing;
                        break;
                    case TaskbarPosition.Right:
                        x = mi.rcWork.right - windowWidth - spacing;
                        y = cursor.Y - windowHeight / 2;
                        if (y < mi.rcWork.top + spacing)
                            y = mi.rcWork.top + spacing;
                        if (y + windowHeight > mi.rcWork.bottom - spacing)
                            y = mi.rcWork.bottom - windowHeight - spacing;
                        break;
                    default:
                        y = autoHide
                            ? mi.rcMonitor.bottom - windowHeight - spacing
                            : mi.rcWork.bottom - windowHeight - spacing;
                        break;
                }

                if (x < mi.rcWork.left) x = mi.rcWork.left;
                if (x + windowWidth > mi.rcWork.right) x = mi.rcWork.right - windowWidth;

                Debug.WriteLine($"PositionAboveTaskbar -> X={x}, Y={y}");

                SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0,
                    SWP_NOSIZE | SWP_NOZORDER | (show ? SWP_SHOWWINDOW : 0));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PositionWindowAboveTaskbar error: {ex.Message}");
            }
        }

        public static void PositionWindowBelowTaskbar(IntPtr hWnd)
        {
            try
            {
                if (!GetWindowRect(hWnd, out RECT wr)) return;
                int windowWidth = wr.right - wr.left;
                int windowHeight = wr.bottom - wr.top;

                if (!GetCursorPos(out POINT cursor)) return;

                IntPtr monitor = MonitorFromPoint(cursor, MONITOR_DEFAULTTONEAREST);
                var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
                if (!GetMonitorInfo(monitor, ref mi)) return;

                float dpiScale = GetDpiScaleForMonitor(monitor);
                int taskbarHeight = (int)(52 * dpiScale);
                int spacing = 99999;
                if (IsTaskbarAutoHide())
                    spacing += (int)(52 * dpiScale);

                TaskbarPosition tbPos = GetTaskbarPosition(mi);
                int x = cursor.X - windowWidth / 2;
                int y;

                switch (tbPos)
                {
                    case TaskbarPosition.Top:
                        y = mi.rcMonitor.top + taskbarHeight + spacing;
                        break;
                    case TaskbarPosition.Bottom:
                        y = mi.rcMonitor.bottom + spacing;
                        break;
                    case TaskbarPosition.Left:
                        x = mi.rcMonitor.left + taskbarHeight + spacing;
                        y = cursor.Y - windowHeight / 2;
                        break;
                    case TaskbarPosition.Right:
                        x = mi.rcMonitor.right - windowWidth - taskbarHeight - spacing;
                        y = cursor.Y - windowHeight / 2;
                        break;
                    default:
                        y = mi.rcMonitor.bottom + spacing;
                        break;
                }

                if (x < mi.rcWork.left) x = mi.rcWork.left;
                if (x + windowWidth > mi.rcWork.right) x = mi.rcWork.right - windowWidth;

                SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PositionWindowBelowTaskbar error: {ex.Message}");
            }
        }

        #endregion
    }
}
