using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using AppGroup.Interop;
using AppGroup.Managers;
using AppGroup.Logging;
using AppGroup.Services;
using AppGroup.Utilities;

namespace AppGroup
{
    public class Program
    {
        public static NativeMethods.POINT InitialClickPos;
        [STAThread]
        
        static void Main(string[] args)
        {
            // Initialize localization with system language
            LocalizationManager.Initialize();
            
            // Set current thread culture
            Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
            
            NativeMethods.GetCursorPos(out InitialClickPos);
            // Register the same message as your receiver
            int msgId = NativeMethods.WM_UPDATE_GROUP;
            string[] cmdArgs = Environment.GetCommandLineArgs();
            bool isSilent = HasSilentFlag(cmdArgs);

            // Check if running without arguments and another instance is already running
            if (cmdArgs.Length <= 1 && !isSilent)
            {
                IntPtr existingMainHWnd = NativeMethods.FindWindow(null!, "App Group");
                if (existingMainHWnd != IntPtr.Zero)
                {
                    NativeMethods.SendString(existingMainHWnd, "__SHOW_MAIN__");
                    return;
                }
            }

            if (!isSilent && cmdArgs.Length > 1)
            {
                IntPtr existingPopupHWnd = NativeMethods.FindWindow(null!, "Popup Window");
                IntPtr existingEditHWnd = NativeMethods.FindWindow(null!, "Edit Group");
                IntPtr existingMainHWnd = NativeMethods.FindWindow(null!, "App Group");

                // Handle existing windows in constructor for faster response
                string command = cmdArgs[1];

                if (command == "EditGroupWindow")
                {
                    // AppGroup.exe EditGroupWindow --id
                    int groupId = JumpListManager.ExtractIdFromCommandLine(cmdArgs);
                    JumpListManager.SaveGroupIdToFile(groupId.ToString());

                    if (existingEditHWnd != IntPtr.Zero)
                    {
                        EditGroupHelper editGroup = new EditGroupHelper("Edit Group", groupId);
                        editGroup.Activate();
                        return;
                    }
                    else if (existingMainHWnd != IntPtr.Zero || existingPopupHWnd != IntPtr.Zero)
                    {
                        return;
                    }
                }

                if (command == "LaunchAll")
                {
                    string targetGroupName = JumpListManager.ExtractGroupNameFromCommandLine(cmdArgs);
                    Task.Run(async () =>
                    {
                        await ConfigService.LaunchAll(targetGroupName);
                    });

                    JumpListManager.InitializeSync();
                    return;
                }

                try
                {
                    int groupId = ConfigService.FindKeyByGroupName(command);
                    JumpListManager.SaveGroupIdToFile(groupId.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to find group ID");
                }

                if (existingPopupHWnd != IntPtr.Zero)
                {
                    NativeMethods.SendString(existingPopupHWnd, command);
                    NativeMethods.ForceForegroundWindow(existingPopupHWnd);
                    JumpListManager.InitializeSync();
                    return;
                }
            }

            WinRT.ComWrappersSupport.InitializeComWrappers();

            if (cmdArgs.Length <= 1 && !isSilent)
            {
                // No arguments provided - check for existing main window instance
                IntPtr existingMainHWnd = NativeMethods.FindWindow(null!, "App Group");
                if (existingMainHWnd != IntPtr.Zero)
                {
                    // Bring existing instance to foreground and exit
                    NativeMethods.SetForegroundWindow(existingMainHWnd);
                    NativeMethods.ShowWindow(existingMainHWnd, NativeMethods.SW_RESTORE);
                }
            }

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }

        private static bool HasSilentFlag(string[] args)
        {
            return JumpListManager.HasSilentFlag(args);
        }
    }
}
