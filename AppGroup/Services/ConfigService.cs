using AppGroup.Logging;
using AppGroup.Models;
using AppGroup.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using File = System.IO.File;

namespace AppGroup.Services
{
    /// <summary>
    /// Service for managing AppGroup configuration using typed POCO classes.
    /// Replaces the legacy JsonConfigHelper with strong typing.
    /// </summary>
    public static class ConfigService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static readonly string _configDir;
        private static readonly string _defaultConfigPath;

        static ConfigService()
        {
            _configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AppGroup");
            _defaultConfigPath = Path.Combine(_configDir, "appgroups.json");
            
            Directory.CreateDirectory(_configDir);
        }

        #region Configuration Loading/Saving

        /// <summary>
        /// Loads the configuration from the default file.
        /// </summary>
        public static AppGroupConfig LoadConfig()
        {
            return LoadConfig(_defaultConfigPath);
        }

        /// <summary>
        /// Loads the configuration from a specific file.
        /// </summary>
        public static AppGroupConfig LoadConfig(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.Debug($"Config file not found: {filePath}, returning empty config");
                    return new AppGroupConfig();
                }

                string jsonContent = File.ReadAllText(filePath);
                var config = DeserializeConfig(jsonContent);
                Logger.Debug($"Loaded config with {config.Groups.Count} groups");
                return config;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load configuration");
                return new AppGroupConfig();
            }
        }

        /// <summary>
        /// Saves the configuration to the default file.
        /// </summary>
        public static bool SaveConfig(AppGroupConfig config)
        {
            return SaveConfig(config, _defaultConfigPath);
        }

        /// <summary>
        /// Saves the configuration to a specific file.
        /// </summary>
        public static bool SaveConfig(AppGroupConfig config, string filePath)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                config.LastModified = DateTime.Now.ToString("o");
                string jsonContent = SerializeConfig(config);
                File.WriteAllText(filePath, jsonContent);
                Logger.Debug($"Saved config with {config.Groups.Count} groups");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save configuration");
                return false;
            }
        }

        /// <summary>
        /// Migrates legacy JSON format to the new typed format.
        /// </summary>
        public static AppGroupConfig MigrateLegacyConfig(string legacyJson)
        {
            try
            {
                var config = new AppGroupConfig();
                var jsonDocument = JsonDocument.Parse(legacyJson);
                
                foreach (var property in jsonDocument.RootElement.EnumerateObject())
                {
                    if (int.TryParse(property.Name, out int groupId))
                    {
                        var groupElement = property.Value;
                        var group = new GroupConfig(groupId);
                        
                        // Map legacy properties
                        if (groupElement.TryGetProperty("groupName", out var groupNameElement))
                            group.Name = groupNameElement.GetString() ?? "Unnamed Group";
                        
                        if (groupElement.TryGetProperty("groupHeader", out var groupHeaderElement))
                            group.ShowHeader = groupHeaderElement.GetBoolean();
                        
                        if (groupElement.TryGetProperty("groupIcon", out var groupIconElement))
                            group.IconPath = groupIconElement.GetString() ?? string.Empty;
                        
                        if (groupElement.TryGetProperty("groupCol", out var groupColElement))
                            group.ColumnCount = groupColElement.GetInt32();
                        
                        if (groupElement.TryGetProperty("showLabels", out var showLabelsElement))
                            group.ShowLabels = showLabelsElement.GetBoolean();
                        
                        if (groupElement.TryGetProperty("labelSize", out var labelSizeElement))
                            group.LabelSize = labelSizeElement.GetInt32();
                        
                        if (groupElement.TryGetProperty("labelPosition", out var labelPositionElement))
                            group.LabelPosition = labelPositionElement.GetString() ?? "Bottom";
                        
                        if (groupElement.TryGetProperty("headerPosition", out var headerPositionElement))
                            group.HeaderPosition = headerPositionElement.GetString() ?? "Top";
                        
                        if (groupElement.TryGetProperty("layout", out var layoutElement))
                            group.Layout = layoutElement.GetString() ?? "Default";
                        
                        if (groupElement.TryGetProperty("showOnTray", out var showOnTrayElement))
                            group.ShowOnTray = showOnTrayElement.GetBoolean();
                        
                        // Map path dictionary to Items
                        if (groupElement.TryGetProperty("path", out var pathElement))
                        {
                            foreach (var pathEntry in pathElement.EnumerateObject())
                            {
                                string path = pathEntry.Name;
                                var pathDetails = pathEntry.Value;
                                
                                string tooltip = pathDetails.TryGetProperty("tooltip", out var tooltipElement) 
                                    ? tooltipElement.GetString() ?? string.Empty 
                                    : string.Empty;
                                
                                string args = pathDetails.TryGetProperty("args", out var argsElement) 
                                    ? argsElement.GetString() ?? string.Empty 
                                    : string.Empty;
                                
                                string icon = pathDetails.TryGetProperty("icon", out var iconElement) 
                                    ? iconElement.GetString() ?? string.Empty 
                                    : string.Empty;
                                
                                group.Items[path] = new AppItemConfig(path, tooltip, args, icon);
                            }
                        }
                        
                        config.Groups[groupId.ToString()] = group;
                    }
                }
                
                Logger.Info("Migrated legacy config to typed format");
                return config;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to migrate legacy config");
                return new AppGroupConfig();
            }
        }

        #endregion

        #region Group Management

        /// <summary>
        /// Gets all groups from the configuration.
        /// </summary>
        public static List<GroupConfig> GetAllGroups()
        {
            var config = LoadConfig();
            return new List<GroupConfig>(config.Groups.Values);
        }

        /// <summary>
        /// Gets a group by ID.
        /// </summary>
        public static GroupConfig? GetGroupById(int groupId)
        {
            var config = LoadConfig();
            config.TryGetGroup(groupId, out var group);
            return group;
        }

        /// <summary>
        /// Gets a group by name.
        /// </summary>
        public static GroupConfig? GetGroupByName(string groupName)
        {
            var config = LoadConfig();
            return config.GetGroupByName(groupName);
        }

        /// <summary>
        /// Adds a new group to the configuration.
        /// </summary>
        public static int AddGroup(GroupConfig group)
        {
            var config = LoadConfig();
            
            if (group.Id <= 0)
                group.Id = config.NextGroupId;
            
            config.AddGroup(group);
            SaveConfig(config);
            
            Logger.Info($"Added group {group.Id}: {group.Name}");
            return group.Id;
        }

        /// <summary>
        /// Updates an existing group.
        /// </summary>
        public static bool UpdateGroup(GroupConfig group)
        {
            var config = LoadConfig();
            
            if (config.Groups.ContainsKey(group.Id.ToString()))
            {
                config.Groups[group.Id.ToString()] = group;
                return SaveConfig(config);
            }
            
            Logger.Warn($"Group {group.Id} not found for update");
            return false;
        }

        /// <summary>
        /// Deletes a group by ID.
        /// </summary>
        public static bool DeleteGroup(int groupId)
        {
            var config = LoadConfig();
            bool removed = config.RemoveGroup(groupId);
            
            if (removed)
            {
                bool saved = SaveConfig(config);
                if (saved)
                {
                    // Also delete the group folder
                    var group = GetGroupById(groupId);
                    if (group != null)
                    {
                        string groupsFolder = Path.Combine(_configDir, "Groups");
                        string groupFolder = Path.Combine(groupsFolder, group.Name);
                        if (Directory.Exists(groupFolder))
                        {
                            try
                            {
                                Directory.Delete(groupFolder, true);
                                Logger.Debug($"Deleted group folder: {groupFolder}");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Failed to delete group folder");
                            }
                        }
                    }
                    Logger.Info($"Deleted group {groupId}");
                    return true;
                }
            }
            
            Logger.Warn($"Failed to delete group {groupId}");
            return false;
        }

        /// <summary>
        /// Duplicates a group.
        /// </summary>
        public static int DuplicateGroup(int groupId)
        {
            var sourceGroup = GetGroupById(groupId);
            if (sourceGroup == null)
            {
                Logger.Warn($"Group {groupId} not found for duplication");
                return -1;
            }

            var config = LoadConfig();
            string newName = config.GetUniqueGroupName($"{sourceGroup.Name} - Copy");
            
            var newGroup = new GroupConfig(config.NextGroupId, newName)
            {
                ShowHeader = sourceGroup.ShowHeader,
                IconPath = sourceGroup.IconPath.Replace(sourceGroup.Name, newName),
                ColumnCount = sourceGroup.ColumnCount,
                ShowLabels = sourceGroup.ShowLabels,
                LabelSize = sourceGroup.LabelSize,
                LabelPosition = sourceGroup.LabelPosition,
                HeaderPosition = sourceGroup.HeaderPosition,
                Layout = sourceGroup.Layout,
                ShowOnTray = sourceGroup.ShowOnTray,
                IsGridIcon = sourceGroup.IsGridIcon,
                EnableAnimations = sourceGroup.EnableAnimations
            };

            // Deep copy items
            foreach (var item in sourceGroup.Items)
            {
                newGroup.Items[item.Key] = new AppItemConfig
                {
                    Path = item.Value.Path,
                    Tooltip = item.Value.Tooltip,
                    Arguments = item.Value.Arguments,
                    Icon = item.Value.Icon.Replace(sourceGroup.Name, newName),
                    RunAsAdmin = item.Value.RunAsAdmin
                };
            }

            config.AddGroup(newGroup);
            SaveConfig(config);
            
            // Copy group folder
            string groupsFolder = Path.Combine(_configDir, "Groups");
            string sourceFolder = Path.Combine(groupsFolder, sourceGroup.Name);
            string destFolder = Path.Combine(groupsFolder, newName);
            
            if (Directory.Exists(sourceFolder))
            {
                try
                {
                    foreach (string filePath in Directory.GetFiles(sourceFolder))
                    {
                        string fileName = Path.GetFileName(filePath);
                        string newFileName = fileName.Replace(sourceGroup.Name, newName);
                        string destPath = Path.Combine(destFolder, newFileName);
                        Directory.CreateDirectory(destFolder);
                        File.Copy(filePath, destPath, true);
                    }
                    Logger.Debug($"Copied group folder from {sourceFolder} to {destFolder}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to copy group folder");
                }
            }

            Logger.Info($"Duplicated group {groupId} to {newGroup.Id}: {newName}");
            return newGroup.Id;
        }

        #endregion

        #region App Item Management

        /// <summary>
        /// Adds an app item to a group.
        /// </summary>
        public static bool AddAppItem(int groupId, AppItemConfig item)
        {
            var group = GetGroupById(groupId);
            if (group == null)
            {
                Logger.Warn($"Group {groupId} not found");
                return false;
            }

            group.Items[item.Path] = item;
            return UpdateGroup(group);
        }

        /// <summary>
        /// Removes an app item from a group.
        /// </summary>
        public static bool RemoveAppItem(int groupId, string path)
        {
            var group = GetGroupById(groupId);
            if (group == null)
            {
                Logger.Warn($"Group {groupId} not found");
                return false;
            }

            if (group.Items.Remove(path))
            {
                return UpdateGroup(group);
            }
            
            Logger.Warn($"App item {path} not found in group {groupId}");
            return false;
        }

        /// <summary>
        /// Updates an app item in a group.
        /// </summary>
        public static bool UpdateAppItem(int groupId, string oldPath, AppItemConfig newItem)
        {
            var group = GetGroupById(groupId);
            if (group == null)
            {
                Logger.Warn($"Group {groupId} not found");
                return false;
            }

            if (group.Items.ContainsKey(oldPath))
            {
                group.Items.Remove(oldPath);
                group.Items[newItem.Path] = newItem;
                return UpdateGroup(group);
            }
            
            Logger.Warn($"App item {oldPath} not found in group {groupId}");
            return false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the default configuration path.
        /// </summary>
        public static string GetDefaultConfigPath(string fileName = "appgroups.json")
        {
            return Path.Combine(_configDir, fileName);
        }

        /// <summary>
        /// Gets the next available group ID.
        /// </summary>
        public static int GetNextGroupId()
        {
            var config = LoadConfig();
            return config.NextGroupId;
        }

        /// <summary>
        /// Checks if a group exists by name.
        /// </summary>
        public static bool GroupExists(string groupName)
        {
            return GetGroupByName(groupName) != null;
        }

        /// <summary>
        /// Checks if a group exists by ID.
        /// </summary>
        public static bool GroupExists(int groupId)
        {
            return GetGroupById(groupId) != null;
        }

        /// <summary>
        /// Finds a group ID by its name.
        /// </summary>
        public static int FindKeyByGroupName(string groupName)
        {
            var group = GetGroupByName(groupName);
            return group?.Id ?? -1;
        }

        /// <summary>
        /// Finds a group name by its ID.
        /// </summary>
        public static string FindGroupNameByKey(int groupId)
        {
            var group = GetGroupById(groupId);
            return group?.Name ?? string.Empty;
        }

        /// <summary>
        /// Launches all applications in a group.
        /// </summary>
        public static async Task LaunchAll(string groupName)
        {
            try
            {
                var group = GetGroupByName(groupName);
                if (group == null)
                {
                    Logger.Warn("Group not found: " + groupName);
                    return;
                }

                var allTasks = new List<Task>();
                
                foreach (var item in group.Items)
                {
                    string path = item.Key;
                    string args = item.Value.Arguments ?? string.Empty;

                    // Skip sub-popup shortcuts
                    if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Check if this is an AppGroup shortcut
                            string? description = ShortcutHelper.GetShortcutDescription(path);
                            if (description != null && description.EndsWith("- AppGroup Shortcut", StringComparison.OrdinalIgnoreCase))
                                continue;
                        }
                        catch { }
                    }

                    allTasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            // CMD wrapper
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c start \"\" \"{path}\" {args}",
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process? process = Process.Start(psi);
                            process?.Close();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to launch " + path);
                        }
                    }));
                }

                await Task.WhenAll(allTasks);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error launching all paths under group: " + groupName);
            }
        }

        /// <summary>
        /// Opens the group folder in Windows Explorer.
        /// </summary>
        public static void OpenGroupFolder(int groupId)
        {
            try
            {
                var group = GetGroupById(groupId);
                if (group == null)
                {
                    Logger.Warn($"Group {groupId} not found");
                    return;
                }

                string groupsFolder = Path.Combine(_configDir, "Groups");
                string groupFolderPath = Path.Combine(groupsFolder, group.Name);

                if (Directory.Exists(groupFolderPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = groupFolderPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Logger.Warn($"Group folder not found: {groupFolderPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error opening group folder");
            }
        }

        #endregion

        #region Serialization Helpers

        internal static AppGroupConfig DeserializeConfig(string json)
        {
            try
            {
                // Try to deserialize as new format
                return JsonSerializer.Deserialize<AppGroupConfig>(json, _jsonOptions) ?? new AppGroupConfig();
            }
            catch (JsonException)
            {
                // If deserialization fails, try to migrate from legacy format
                return MigrateLegacyConfig(json);
            }
        }

        internal static string SerializeConfig(AppGroupConfig config)
        {
            return JsonSerializer.Serialize(config, _jsonOptions);
        }

        #endregion
    }
}
