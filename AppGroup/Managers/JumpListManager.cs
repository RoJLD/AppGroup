using AppGroup.Logging;
using AppGroup.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace AppGroup.Managers
{
    /// <summary>
    /// Manages the creation and updating of the Windows JumpList for AppGroup.
    /// The JumpList provides quick access to group editing and launch all functionality.
    /// </summary>
    public static class JumpListManager
    {
        /// <summary>
        /// Initializes the JumpList synchronously.
        /// </summary>
        public static void InitializeSync()
        {
            try
            {
                Task.Run(async () => await InitializeAsync()).Wait();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Sync jump list initialization failed");
            }
        }

        /// <summary>
        /// Initializes the JumpList asynchronously based on command line arguments.
        /// </summary>
        public static async Task InitializeAsync()
        {
            try
            {
                string[] cmdArgs = Environment.GetCommandLineArgs();
                JumpList jumpList = await JumpList.LoadCurrentAsync();

                Logger.Debug($"Jump list initialization started with args: {string.Join(", ", cmdArgs)}");

                // Only modify jump list when there ARE arguments
                if (cmdArgs.Length > 1)
                {
                    string command = cmdArgs[1];
                    Logger.Debug($"Processing command: '{command}'");

                    jumpList.Items.Clear();

                    if (command == "EditGroupWindow")
                    {
                        Logger.Debug("Creating jump list for EditGroupWindow");
                        var jumpListItem = CreateJumpListItemTask();
                        var launchAllItem = CreateLaunchAllJumpListItem();

                        jumpList.Items.Add(jumpListItem);
                        jumpList.Items.Add(launchAllItem);
                    }
                    else if (command == "LaunchAll")
                    {
                        Logger.Debug("Creating jump list for LaunchAll");
                    }
                    else
                    {
                        // This is a group name
                        Logger.Debug($"Creating jump list for group name: '{command}'");

                        if (ConfigService.GroupExists(command))
                        {
                            var jumpListItem = CreateJumpListItemTask();
                            var launchAllItem = CreateLaunchAllJumpListItem();

                            jumpList.Items.Add(jumpListItem);
                            jumpList.Items.Add(launchAllItem);

                            Logger.Debug($"Jump list items created for group '{command}'");
                        }
                        else
                        {
                            Logger.Warn($"Group '{command}' does not exist in configuration");
                        }
                    }

                    await jumpList.SaveAsync();
                    Logger.Debug("Jump list saved successfully");
                }
                else
                {
                    Logger.Debug("No arguments provided, jump list not modified");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Jump list initialization failed");
            }
        }

        /// <summary>
        /// Creates a JumpList item for editing the current group.
        /// </summary>
        public static JumpListItem CreateJumpListItemTask()
        {
            try
            {
                string[] cmdArgs = Environment.GetCommandLineArgs();
                Logger.Debug($"CreateJumpListItemTask called with args: {string.Join(", ", cmdArgs)}");

                if (cmdArgs.Length > 1)
                {
                    string command = cmdArgs[1];
                    Logger.Debug($"Processing command: '{command}'");

                    if (command == "EditGroupWindow")
                    {
                        int groupId = ExtractIdFromCommandLine(cmdArgs);
                        SaveGroupIdToFile(groupId.ToString());
                        var taskItem = JumpListItem.CreateWithArguments("EditGroupWindow --id=" + groupId, "Edit this Group");
                        Logger.Debug($"Created EditGroupWindow jump list item with ID: {groupId}");
                        return taskItem;
                    }
                    else if (command != "LaunchAll")
                    {
                        try
                        {
                            int groupId = ConfigService.FindKeyByGroupName(command);
                            SaveGroupIdToFile(groupId.ToString());
                            var taskItem = JumpListItem.CreateWithArguments("EditGroupWindow --id=" + groupId, "Edit this Group");
                            Logger.Debug($"Created jump list item for group '{command}' with ID: {groupId}");
                            return taskItem;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to find group ID");
                        }
                    }
                }

                Logger.Debug("Using fallback jump list item");
                return JumpListItem.CreateWithArguments("EditGroupWindow --id=0", "Edit Group");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create edit jump list item");
                return JumpListItem.CreateWithArguments("EditGroupWindow --id=0", "Edit Group");
            }
        }

        /// <summary>
        /// Creates a JumpList item for launching all applications in the current group.
        /// </summary>
        public static JumpListItem CreateLaunchAllJumpListItem()
        {
            try
            {
                string[] cmdArgs = Environment.GetCommandLineArgs();
                Logger.Debug($"CreateLaunchAllJumpListItem called with args: {string.Join(", ", cmdArgs)}");

                if (cmdArgs.Length > 1)
                {
                    string command = cmdArgs[1];

                    if (command == "EditGroupWindow")
                    {
                        int groupId = ExtractIdFromCommandLine(cmdArgs);
                        var taskItem = JumpListItem.CreateWithArguments($"LaunchAll --groupId={groupId}", "Launch All");
                        Logger.Debug($"Created LaunchAll item for EditGroupWindow with ID: {groupId}");
                        return taskItem;
                    }
                    else if (command != "LaunchAll")
                    {
                        string groupName = command;
                        var taskItem = JumpListItem.CreateWithArguments($"LaunchAll --groupName=\"{groupName}\"", "Launch All");
                        Logger.Debug($"Created LaunchAll item for group: '{groupName}'");
                        return taskItem;
                    }
                }

                Logger.Debug("Using fallback LaunchAll item");
                return JumpListItem.CreateWithArguments("LaunchAll", "Launch All");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create launch all jump list item");
                return JumpListItem.CreateWithArguments("LaunchAll", "Launch All");
            }
        }

        #region Utility Methods

        /// <summary>
        /// Extracts the group name from command line arguments.
        /// </summary>
        public static string ExtractGroupNameFromCommandLine(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--groupName="))
                    {
                        return arg.Substring(12).Trim('"');
                    }
                    else if (arg.StartsWith("--groupId="))
                    {
                        if (int.TryParse(arg.Substring(10), out int groupId))
                        {
                            return ConfigService.FindGroupNameByKey(groupId);
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error extracting group name");
                return string.Empty;
            }
        }

        /// <summary>
        /// Saves the group ID to a file for persistence.
        /// </summary>
        public static void SaveGroupIdToFile(string groupId)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(appDataPath, "AppGroup", "lastEdit");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "");
                File.WriteAllText(filePath, groupId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save group ID");
            }
        }

        /// <summary>
        /// Extracts the ID from command line arguments.
        /// </summary>
        public static int ExtractIdFromCommandLine(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--id="))
                    {
                        if (int.TryParse(arg.Substring(5), out int id))
                        {
                            return id;
                        }
                    }
                }
                return ConfigService.GetNextGroupId();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error extracting ID");
                return ConfigService.GetNextGroupId();
            }
        }

        /// <summary>
        /// Checks if the silent flag is present in command line arguments.
        /// </summary>
        public static bool HasSilentFlag(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    if (arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking silent flag");
                return false;
            }
        }

        #endregion
    }
}
