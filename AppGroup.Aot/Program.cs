using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;
using AppGroup.Aot.Json;
using AppGroup.Aot.Models;

namespace AppGroup.Aot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AppGroup Native AOT Launcher");
            Console.WriteLine("============================");

            // Handle command line arguments
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "list":
                        ListGroups();
                        break;
                    case "add":
                        if (args.Length > 1)
                            AddGroup(args[1]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot add <groupName>");
                        break;
                    case "additem":
                        if (args.Length > 2)
                            AddItem(args[1], args[2], args.Length > 3 ? args[3] : "", args.Length > 4 ? args[4] : "", args.Length > 5 ? args[5] : "");
                        else
                            Console.WriteLine("Usage: AppGroup.Aot additem <groupName> <shortcutPath> [name] [arguments] [icon]");
                        break;
                    case "edititem":
                        if (args.Length > 2)
                            EditItem(args[1], args[2], args.Skip(3).ToArray());
                        else
                            Console.WriteLine("Usage: AppGroup.Aot edititem <groupName> <shortcutPath> [--name <name>] [--args <arguments>] [--icon <iconPath>]");
                        break;
                    case "moveitem":
                        if (args.Length > 3)
                            MoveItem(args[1], args[2], args[3]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot moveitem <groupName> <shortcutPath> <targetIndex>");
                        break;
                    case "removeitem":
                        if (args.Length > 2)
                            RemoveItem(args[1], args[2]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot removeitem <groupName> <shortcutPath>");
                        break;
                    case "export":
                        if (args.Length > 2)
                            ExportGroup(args[1], args[2]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot export <groupName> <filePath>");
                        break;
                    case "import":
                        if (args.Length > 1)
                            ImportGroup(args[1], args.Length > 2 ? args[2] : "");
                        else
                            Console.WriteLine("Usage: AppGroup.Aot import <filePath> [newGroupName]");
                        break;
                    case "listitems":
                        if (args.Length > 1)
                            ListItems(args[1]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot listitems <groupName>");
                        break;
                    case "launch":
                        if (args.Length > 1)
                            LaunchGroup(args[1]);
                        else
                            Console.WriteLine("Usage: AppGroup.Aot launch <groupName>");
                        break;
                    case "--help":
                    case "-h":
                        ShowHelp();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {args[0]}");
                        ShowHelp();
                        break;
                }
            }
            else
            {
                // Interactive mode
                RunInteractiveMode();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("AppGroup.Aot - Native AOT Shortcut Group Launcher");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("  AppGroup.Aot                    - Interactive mode");
            Console.WriteLine("  AppGroup.Aot list               - List all groups");
            Console.WriteLine("  AppGroup.Aot add <name>         - Add a new group");
            Console.WriteLine("  AppGroup.Aot additem <group> <path> [name] [args] [icon] - Add shortcut to group");
            Console.WriteLine("  AppGroup.Aot edititem <group> <path> [--name <n>] [--args <a>] [--icon <i>] - Edit a shortcut");
            Console.WriteLine("  AppGroup.Aot moveitem <group> <path> <index> - Reorder a shortcut (0-based)");
            Console.WriteLine("  AppGroup.Aot removeitem <group> <path> - Remove shortcut from group");
            Console.WriteLine("  AppGroup.Aot listitems <group>  - List shortcuts in group");
            Console.WriteLine("  AppGroup.Aot export <group> <file>  - Export a group to a JSON file");
            Console.WriteLine("  AppGroup.Aot import <file> [name]   - Import a group from a JSON file");
            Console.WriteLine("  AppGroup.Aot launch <name>      - Launch all shortcuts in a group");
            Console.WriteLine("  AppGroup.Aot --help             - Show this help");
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("Interactive mode (type 'help' for commands, 'exit' to quit)");

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                switch (command)
                {
                    case "exit":
                    case "quit":
                        return;
                    case "help":
                        ShowHelp();
                        break;
                    case "list":
                        ListGroups();
                        break;
                    case "add":
                        if (parts.Length > 1)
                            AddGroup(parts[1]);
                        else
                            Console.WriteLine("Please specify a group name");
                        break;
                    case "additem":
                        if (parts.Length > 2)
                            AddItem(parts[1], parts[2], parts.Length > 3 ? parts[3] : "", parts.Length > 4 ? parts[4] : "", parts.Length > 5 ? parts[5] : "");
                        else
                            Console.WriteLine("Please specify group name and shortcut path");
                        break;
                    case "edititem":
                        if (parts.Length > 2)
                            EditItem(parts[1], parts[2], parts.Skip(3).ToArray());
                        else
                            Console.WriteLine("Please specify group name and shortcut path");
                        break;
                    case "moveitem":
                        if (parts.Length > 3)
                            MoveItem(parts[1], parts[2], parts[3]);
                        else
                            Console.WriteLine("Please specify group name, shortcut path and target index");
                        break;
                    case "removeitem":
                        if (parts.Length > 2)
                            RemoveItem(parts[1], parts[2]);
                        else
                            Console.WriteLine("Please specify group name and shortcut path");
                        break;
                    case "export":
                        if (parts.Length > 2)
                            ExportGroup(parts[1], parts[2]);
                        else
                            Console.WriteLine("Please specify group name and file path");
                        break;
                    case "import":
                        if (parts.Length > 1)
                            ImportGroup(parts[1], parts.Length > 2 ? parts[2] : "");
                        else
                            Console.WriteLine("Please specify a file path");
                        break;
                    case "listitems":
                        if (parts.Length > 1)
                            ListItems(parts[1]);
                        else
                            Console.WriteLine("Please specify a group name");
                        break;
                    case "launch":
                        if (parts.Length > 1)
                            LaunchGroup(parts[1]);
                        else
                            Console.WriteLine("Please specify a group name");
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static void ListGroups()
        {
            var config = LoadConfig();

            if (config.Groups.Count == 0)
            {
                Console.WriteLine("No groups found.");
                return;
            }

            Console.WriteLine($"Found {config.Groups.Count} group(s):");
            Console.WriteLine();

            foreach (var group in config.Groups.Values)
            {
                Console.WriteLine($"[{group.Id}] {group.Name}");
                Console.WriteLine($"    Items: {group.Items.Count}");
                if (!string.IsNullOrEmpty(group.IconPath))
                    Console.WriteLine($"    Icon: {group.IconPath}");
                Console.WriteLine();
            }
        }

        static void AddGroup(string groupName)
        {
            var config = LoadConfig();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                Console.WriteLine("Group name cannot be empty.");
                return;
            }

            // Check if group already exists
            if (config.GroupNameExists(groupName))
            {
                Console.WriteLine($"A group named '{groupName}' already exists.");
                return;
            }

            // Create new group
            var newGroup = new GroupConfig(config.NextGroupId, groupName);

            config.AddGroup(newGroup);
            SaveConfig(config);

            Console.WriteLine($"Group '{groupName}' added successfully with ID {newGroup.Id}.");
        }

        static void AddItem(string groupName, string shortcutPath, string name, string arguments, string icon)
        {
            var config = LoadConfig();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                Console.WriteLine("Group name cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(shortcutPath))
            {
                Console.WriteLine("Shortcut path cannot be empty.");
                return;
            }

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            // Check if item already exists (by path)
            if (group.Items.ContainsKey(shortcutPath))
            {
                Console.WriteLine($"Shortcut '{shortcutPath}' already exists in group '{groupName}'.");
                return;
            }

            // Create new item
            var newItem = new AppItemConfig
            {
                Path = shortcutPath,
                Tooltip = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(shortcutPath) : name,
                Arguments = arguments,
                Icon = icon
            };

            group.Items[shortcutPath] = newItem;
            // Maintain order
            if (!group.ItemOrder.Contains(shortcutPath))
                group.ItemOrder.Add(shortcutPath);

            SaveConfig(config);

            Console.WriteLine($"Shortcut '{newItem.Tooltip}' added to group '{groupName}'.");
        }

        static void EditItem(string groupName, string shortcutPath, string[] options)
        {
            var config = LoadConfig();

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            if (!group.Items.TryGetValue(shortcutPath, out var item))
            {
                Console.WriteLine($"Shortcut '{shortcutPath}' not found in group '{groupName}'.");
                return;
            }

            // Parse selective flags: only the fields the user provides are changed.
            string? newName = GetFlagValue(options, "--name");
            string? newArgs = GetFlagValue(options, "--args");
            string? newIcon = GetFlagValue(options, "--icon");

            if (newName == null && newArgs == null && newIcon == null)
            {
                Console.WriteLine("Nothing to edit. Provide at least one of --name, --args or --icon.");
                return;
            }

            if (newName != null) item.Tooltip = newName;
            if (newArgs != null) item.Arguments = newArgs;
            if (newIcon != null) item.Icon = newIcon;

            SaveConfig(config);

            Console.WriteLine($"Shortcut '{shortcutPath}' updated in group '{groupName}'.");
        }

        static void MoveItem(string groupName, string shortcutPath, string targetIndexRaw)
        {
            var config = LoadConfig();

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            if (!group.Items.ContainsKey(shortcutPath))
            {
                Console.WriteLine($"Shortcut '{shortcutPath}' not found in group '{groupName}'.");
                return;
            }

            if (!int.TryParse(targetIndexRaw, out int targetIndex))
            {
                Console.WriteLine($"Invalid target index: '{targetIndexRaw}'. Expected a number.");
                return;
            }

            // Rebuild ItemOrder if it is missing/out of sync with the items.
            if (group.ItemOrder.Count != group.Items.Count)
            {
                group.ItemOrder = new List<string>(group.Items.Keys);
            }

            // Clamp the target to a valid range (0-based).
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex >= group.ItemOrder.Count) targetIndex = group.ItemOrder.Count - 1;

            group.ItemOrder.Remove(shortcutPath);
            group.ItemOrder.Insert(targetIndex, shortcutPath);

            SaveConfig(config);

            Console.WriteLine($"Shortcut '{shortcutPath}' moved to position {targetIndex} in group '{groupName}'.");
        }

        static void ExportGroup(string groupName, string filePath)
        {
            var config = LoadConfig();

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            try
            {
                string json = JsonSerializer.Serialize(group, AppJsonContext.Default.GroupConfig);
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Group '{groupName}' exported to '{filePath}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting group: {ex.Message}");
            }
        }

        static void ImportGroup(string filePath, string newGroupName)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File '{filePath}' not found.");
                return;
            }

            GroupConfig? imported;
            try
            {
                string json = File.ReadAllText(filePath);
                imported = JsonSerializer.Deserialize(json, AppJsonContext.Default.GroupConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading group file: {ex.Message}");
                return;
            }

            if (imported == null)
            {
                Console.WriteLine("The file did not contain a valid group.");
                return;
            }

            var config = LoadConfig();

            // Resolve the final name, avoiding collisions with existing groups.
            string desiredName = string.IsNullOrWhiteSpace(newGroupName) ? imported.Name : newGroupName;
            imported.Name = config.GetUniqueGroupName(desiredName);

            // Assign a fresh ID so we never overwrite an existing group.
            imported.Id = config.NextGroupId;
            config.AddGroup(imported);

            SaveConfig(config);

            Console.WriteLine($"Group imported as '{imported.Name}' (ID {imported.Id}) with {imported.Items.Count} item(s).");
        }

        /// <summary>
        /// Returns the value following a flag (e.g. "--name" -> next token), or null if absent.
        /// </summary>
        static string? GetFlagValue(string[] options, string flag)
        {
            for (int i = 0; i < options.Length - 1; i++)
            {
                if (options[i].Equals(flag, StringComparison.OrdinalIgnoreCase))
                    return options[i + 1];
            }
            return null;
        }

        static void RemoveItem(string groupName, string shortcutPath)
        {
            var config = LoadConfig();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                Console.WriteLine("Group name cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(shortcutPath))
            {
                Console.WriteLine("Shortcut path cannot be empty.");
                return;
            }

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            if (!group.Items.ContainsKey(shortcutPath))
            {
                Console.WriteLine($"Shortcut '{shortcutPath}' not found in group '{groupName}'.");
                return;
            }

            group.Items.Remove(shortcutPath);
            group.ItemOrder.Remove(shortcutPath);

            SaveConfig(config);

            Console.WriteLine($"Shortcut '{shortcutPath}' removed from group '{groupName}'.");
        }

        static void ListItems(string groupName)
        {
            var config = LoadConfig();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                Console.WriteLine("Group name cannot be empty.");
                return;
            }

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            if (group.Items.Count == 0)
            {
                Console.WriteLine($"Group '{groupName}' has no items.");
                return;
            }

            Console.WriteLine($"Items in group '{groupName}' (ID {group.Id}):");
            Console.WriteLine();

            // Use ItemOrder if available, otherwise just iterate dictionary
            var paths = group.ItemOrder.Count > 0 ? group.ItemOrder : new List<string>(group.Items.Keys);

            foreach (var path in paths)
            {
                if (group.Items.TryGetValue(path, out var item))
                {
                    Console.WriteLine($"  - {item.Tooltip}");
                    Console.WriteLine($"    Path: {item.Path}");
                    if (!string.IsNullOrEmpty(item.Arguments))
                        Console.WriteLine($"    Arguments: {item.Arguments}");
                    Console.WriteLine();
                }
            }
        }

        static void LaunchGroup(string groupName)
        {
            var config = LoadConfig();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                Console.WriteLine("Group name cannot be empty.");
                return;
            }

            var group = config.GetGroupByName(groupName);
            if (group == null)
            {
                Console.WriteLine($"Group '{groupName}' not found.");
                return;
            }

            Console.WriteLine($"Launching {group.Items.Count} items from group '{group.Name}'...");

            foreach (var item in group.Items.Values)
            {
                try
                {
                    Console.WriteLine($"  Launching: {item.Tooltip} ({item.Path})");

                    var psi = new ProcessStartInfo
                    {
                        FileName = item.Path,
                        Arguments = item.Arguments,
                        UseShellExecute = true
                    };

                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error launching {item.Path}: {ex.Message}");
                }
            }

            Console.WriteLine("Launch complete.");
        }

        static AppGroupConfig LoadConfig()
        {
            string configPath = GetConfigFilePath();

            if (!File.Exists(configPath))
            {
                // Create default config
                var config = new AppGroupConfig();
                SaveConfig(config);
                return config;
            }

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize(json, AppJsonContext.Default.AppGroupConfig) ?? new AppGroupConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                return new AppGroupConfig();
            }
        }

        static void SaveConfig(AppGroupConfig config)
        {
            string configPath = GetConfigFilePath();
            string json = JsonSerializer.Serialize(config, AppJsonContext.Default.AppGroupConfig);

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(configPath, json);
        }

        static string GetConfigFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appGroupDir = Path.Combine(appData, "AppGroup");
            // Must match the WPF app's ConfigService default file name so both
            // the GUI and this CLI share the exact same configuration.
            return Path.Combine(appGroupDir, "appgroups.json");
        }
    }
}