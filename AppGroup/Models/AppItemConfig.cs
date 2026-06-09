using System.Text.Json.Serialization;

namespace AppGroup.Models
{
    /// <summary>
    /// Represents a single application/item within a group.
    /// </summary>
    public class AppItemConfig
    {
        /// <summary>
        /// Path to the executable or shortcut.
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Tooltip text for this item.
        /// </summary>
        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; } = string.Empty;

        /// <summary>
        /// Command line arguments for launching.
        /// </summary>
        [JsonPropertyName("args")]
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Custom icon path for this item.
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Whether to run as administrator.
        /// </summary>
        [JsonPropertyName("runAsAdmin")]
        public bool RunAsAdmin { get; set; } = false;

        /// <summary>
        /// Whether this is a subgroup reference.
        /// </summary>
        [JsonIgnore]
        public bool IsSubGroup { get; set; } = false;

        /// <summary>
        /// For subgroups: the referenced group ID.
        /// </summary>
        [JsonPropertyName("subGroupId")]
        public int SubGroupId { get; set; } = -1;

        public AppItemConfig() { }

        public AppItemConfig(string path, string tooltip = "", string args = "", string icon = "")
        {
            Path = path;
            Tooltip = tooltip;
            Arguments = args;
            Icon = icon;
        }

        public override bool Equals(object? obj)
        {
            return obj is AppItemConfig other &&
                   Path == other.Path &&
                   Tooltip == other.Tooltip &&
                   Arguments == other.Arguments &&
                   Icon == other.Icon;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Path, Tooltip, Arguments, Icon);
        }
    }
}
