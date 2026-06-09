using AppGroup.Interop;
using AppGroup.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AppGroup.Utilities
{
    /// <summary>
    /// Helper class for creating, reading, and updating Windows shortcuts (.lnk files).
    /// Uses P/Invoke instead of IWshRuntimeLibrary COM interop.
    /// </summary>
    public static class ShortcutHelper
    {
        private const int MAX_PATH = 260;

        /// <summary>
        /// Creates a new shortcut file.
        /// </summary>
        /// <param name="shortcutPath">Path where the shortcut will be created</param>
        /// <param name="targetPath">Target executable or file path</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="description">Shortcut description</param>
        /// <param name="workingDirectory">Working directory for the shortcut</param>
        /// <param name="iconPath">Path to icon file (optional)</param>
        /// <param name="iconIndex">Icon index (default: 0)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool CreateShortcut(
            string shortcutPath,
            string targetPath,
            string arguments = "",
            string description = "",
            string workingDirectory = "",
            string iconPath = "",
            int iconIndex = 0)
        {
            try
            {
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(shortcutPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create shell link instance
                var shellLink = NativeMethods.CreateShellLink();
                
                // Set shortcut properties
                shellLink.SetPath(targetPath);
                
                if (!string.IsNullOrEmpty(arguments))
                {
                    shellLink.SetArguments(arguments);
                }
                
                if (!string.IsNullOrEmpty(description))
                {
                    shellLink.SetDescription(description);
                }
                
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    shellLink.SetWorkingDirectory(workingDirectory);
                }
                
                if (!string.IsNullOrEmpty(iconPath))
                {
                    shellLink.SetIconLocation(iconPath, iconIndex);
                }

                // Save the shortcut
                NativeMethods.SaveShellLink(shellLink, shortcutPath);
                
                Logger.Debug($"Created shortcut: {shortcutPath} -> {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create shortcut");
                return false;
            }
        }

        /// <summary>
        /// Reads an existing shortcut file.
        /// </summary>
        /// <param name="shortcutPath">Path to the shortcut file</param>
        /// <param name="targetPath">Output: target path</param>
        /// <param name="arguments">Output: command line arguments</param>
        /// <param name="description">Output: description</param>
        /// <param name="workingDirectory">Output: working directory</param>
        /// <param name="iconPath">Output: icon path</param>
        /// <param name="iconIndex">Output: icon index</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool ReadShortcut(
            string shortcutPath,
            out string targetPath,
            out string arguments,
            out string description,
            out string workingDirectory,
            out string iconPath,
            out int iconIndex)
        {
            targetPath = string.Empty;
            arguments = string.Empty;
            description = string.Empty;
            workingDirectory = string.Empty;
            iconPath = string.Empty;
            iconIndex = 0;

            try
            {
                if (!File.Exists(shortcutPath))
                {
                    Logger.Warn($"Shortcut file not found: {shortcutPath}");
                    return false;
                }

                var shellLink = NativeMethods.LoadShellLink(shortcutPath);

                // Get target path
                var targetBuffer = new StringBuilder(MAX_PATH);
                NativeMethods._WIN32_FIND_DATAW findData = new NativeMethods._WIN32_FIND_DATAW();
                shellLink.GetPath(targetBuffer, targetBuffer.Capacity, ref findData, 0);
                targetPath = targetBuffer.ToString();

                // Get arguments
                var argsBuffer = new StringBuilder(MAX_PATH);
                shellLink.GetArguments(argsBuffer, argsBuffer.Capacity);
                arguments = argsBuffer.ToString();

                // Get description
                var descBuffer = new StringBuilder(MAX_PATH);
                shellLink.GetDescription(descBuffer, descBuffer.Capacity);
                description = descBuffer.ToString();

                // Get working directory
                var workDirBuffer = new StringBuilder(MAX_PATH);
                shellLink.GetWorkingDirectory(workDirBuffer, workDirBuffer.Capacity);
                workingDirectory = workDirBuffer.ToString();

                // Get icon location
                var iconBuffer = new StringBuilder(MAX_PATH);
                shellLink.GetIconLocation(iconBuffer, iconBuffer.Capacity, out iconIndex);
                iconPath = iconBuffer.ToString();

                Logger.Debug($"Read shortcut: {shortcutPath} -> {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to read shortcut");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing shortcut file.
        /// </summary>
        public static bool UpdateShortcut(
            string shortcutPath,
            string? targetPath = null,
            string? arguments = null,
            string? description = null,
            string? workingDirectory = null,
            string? iconPath = null,
            int? iconIndex = null)
        {
            try
            {
                if (!File.Exists(shortcutPath))
                {
                    Logger.Warn($"Shortcut file not found for update: {shortcutPath}");
                    return false;
                }

                var shellLink = NativeMethods.LoadShellLink(shortcutPath);

                // Update properties if specified
                if (targetPath != null)
                {
                    shellLink.SetPath(targetPath);
                }

                if (arguments != null)
                {
                    shellLink.SetArguments(arguments);
                }

                if (description != null)
                {
                    shellLink.SetDescription(description);
                }

                if (workingDirectory != null)
                {
                    shellLink.SetWorkingDirectory(workingDirectory);
                }

                if (iconPath != null)
                {
                    shellLink.SetIconLocation(iconPath, iconIndex ?? 0);
                }

                // Save the updated shortcut
                NativeMethods.SaveShellLink(shellLink, shortcutPath);
                
                Logger.Debug($"Updated shortcut: {shortcutPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update shortcut");
                return false;
            }
        }

        /// <summary>
        /// Gets the target path of a shortcut.
        /// </summary>
        public static string? GetShortcutTarget(string shortcutPath)
        {
            try
            {
                var shellLink = NativeMethods.LoadShellLink(shortcutPath);
                var buffer = new StringBuilder(MAX_PATH);
                NativeMethods._WIN32_FIND_DATAW findData = new NativeMethods._WIN32_FIND_DATAW();
                shellLink.GetPath(buffer, buffer.Capacity, ref findData, 0);
                return buffer.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get shortcut target");
                return null;
            }
        }

        /// <summary>
        /// Gets the arguments of a shortcut.
        /// </summary>
        public static string? GetShortcutArguments(string shortcutPath)
        {
            try
            {
                var shellLink = NativeMethods.LoadShellLink(shortcutPath);
                var buffer = new StringBuilder(MAX_PATH);
                shellLink.GetArguments(buffer, buffer.Capacity);
                return buffer.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get shortcut arguments");
                return null;
            }
        }

        /// <summary>
        /// Gets the description of a shortcut.
        /// </summary>
        public static string? GetShortcutDescription(string shortcutPath)
        {
            try
            {
                var shellLink = NativeMethods.LoadShellLink(shortcutPath);
                var buffer = new StringBuilder(MAX_PATH);
                shellLink.GetDescription(buffer, buffer.Capacity);
                return buffer.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get shortcut description");
                return null;
            }
        }

        /// <summary>
        /// Gets the working directory of a shortcut.
        /// </summary>
        public static string? GetShortcutWorkingDirectory(string shortcutPath)
        {
            try
            {
                var shellLink = NativeMethods.LoadShellLink(shortcutPath);
                var buffer = new StringBuilder(MAX_PATH);
                shellLink.GetWorkingDirectory(buffer, buffer.Capacity);
                return buffer.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get shortcut working directory");
                return null;
            }
        }

        /// <summary>
        /// Gets the icon location of a shortcut.
        /// </summary>
        public static (string IconPath, int IconIndex) GetShortcutIconLocation(string shortcutPath)
        {
            try
            {
                var shellLink = NativeMethods.LoadShellLink(shortcutPath);
                var buffer = new StringBuilder(MAX_PATH);
                shellLink.GetIconLocation(buffer, buffer.Capacity, out int iconIndex);
                return (buffer.ToString(), iconIndex);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get shortcut icon location");
                return (string.Empty, 0);
            }
        }
    }
}
