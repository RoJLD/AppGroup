using System.Collections.Generic;
using System.Text.Json.Serialization;
using AppGroup.Aot.Json;

namespace AppGroup.Aot.Models
{
    /// <summary>
    /// Root configuration object for AppGroup.
    /// Contains all groups and global settings.
    /// </summary>
    [JsonConverter(typeof(AppGroupConfigJsonConverter))]
    public class AppGroupConfig
    {
        /// <summary>
        /// Version of the configuration format.
        /// </summary>
        [JsonPropertyName("configVersion")]
        public string ConfigVersion { get; set; } = "1.0";

        /// <summary>
        /// Last modified timestamp.
        /// </summary>
        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; } = string.Empty;

        /// <summary>
        /// All groups in the configuration.
        /// Key: group ID (string), Value: GroupConfig.
        /// Serialized flat at the root level by <see cref="AppGroupConfigJsonConverter"/>
        /// (not via [JsonExtensionData], which is unsupported with a strongly-typed value
        /// and incompatible with the AOT source generator).
        /// </summary>
        public Dictionary<string, GroupConfig> Groups { get; set; } = new Dictionary<string, GroupConfig>();

        /// <summary>
        /// Gets the next available group ID.
        /// </summary>
        [JsonIgnore]
        public int NextGroupId
        {
            get
            {
                if (Groups.Count == 0)
                    return 1;

                int maxId = 0;
                foreach (var kvp in Groups)
                {
                    if (int.TryParse(kvp.Key, out int id) && id > maxId)
                        maxId = id;
                }
                return maxId + 1;
            }
        }

        public AppGroupConfig() { }

        /// <summary>
        /// Adds a new group to the configuration.
        /// </summary>
        public void AddGroup(GroupConfig group)
        {
            if (group.Id <= 0)
                group.Id = NextGroupId;

            Groups[group.Id.ToString()] = group;
        }

        /// <summary>
        /// Removes a group by ID.
        /// </summary>
        public bool RemoveGroup(int groupId)
        {
            return Groups.Remove(groupId.ToString());
        }

        /// <summary>
        /// Gets a group by ID.
        /// </summary>
        public bool TryGetGroup(int groupId, out GroupConfig? group)
        {
            group = null;
            if (Groups.TryGetValue(groupId.ToString(), out var existingGroup))
            {
                group = existingGroup;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a group by name.
        /// </summary>
        public GroupConfig? GetGroupByName(string groupName)
        {
            foreach (var kvp in Groups)
            {
                if (kvp.Value.Name.Equals(groupName, System.StringComparison.Ordinal))
                    return kvp.Value;
            }
            return null;
        }

        /// <summary>
        /// Checks if a group name exists.
        /// </summary>
        public bool GroupNameExists(string groupName)
        {
            return GetGroupByName(groupName) != null;
        }

        /// <summary>
        /// Gets a unique group name by appending a number if necessary.
        /// </summary>
        public string GetUniqueGroupName(string baseName)
        {
            if (!GroupNameExists(baseName))
                return baseName;

            int counter = 2;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName} ({counter++})";
            } while (GroupNameExists(uniqueName));

            return uniqueName;
        }
    }
}