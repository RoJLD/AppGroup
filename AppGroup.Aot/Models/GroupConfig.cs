using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppGroup.Aot.Models
{
    /// <summary>
    /// Represents a group of applications with customizable appearance and behavior.
    /// </summary>
    public class GroupConfig
    {
        /// <summary>
        /// Unique identifier for the group.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; } = -1;

        /// <summary>
        /// Display name of the group.
        /// </summary>
        [JsonPropertyName("groupName")]
        public string Name { get; set; } = "New Group";

        /// <summary>
        /// Whether to show the group header.
        /// </summary>
        [JsonPropertyName("groupHeader")]
        public bool ShowHeader { get; set; } = true;

        /// <summary>
        /// Path to the group's icon.
        /// </summary>
        [JsonPropertyName("groupIcon")]
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// Number of columns for grid layout.
        /// </summary>
        [JsonPropertyName("groupCol")]
        public int ColumnCount { get; set; } = 3;

        /// <summary>
        /// Whether to show labels for items.
        /// </summary>
        [JsonPropertyName("showLabels")]
        public bool ShowLabels { get; set; } = true;

        /// <summary>
        /// Font size for labels.
        /// </summary>
        [JsonPropertyName("labelSize")]
        public int LabelSize { get; set; } = 12;

        /// <summary>
        /// Position of labels relative to icons.
        /// </summary>
        [JsonPropertyName("labelPosition")]
        public string LabelPosition { get; set; } = "Bottom";

        /// <summary>
        /// Position of the group header.
        /// </summary>
        [JsonPropertyName("headerPosition")]
        public string HeaderPosition { get; set; } = "Top";

        /// <summary>
        /// Layout type for the group.
        /// </summary>
        [JsonPropertyName("layout")]
        public string Layout { get; set; } = "Default";

        /// <summary>
        /// Whether to show this group in the system tray.
        /// </summary>
        [JsonPropertyName("showOnTray")]
        public bool ShowOnTray { get; set; } = true;

        /// <summary>
        /// Whether to use accent color for the background.
        /// </summary>
        [JsonPropertyName("useAccentColor")]
        public bool UseAccentColor { get; set; } = true;

        /// <summary>
        /// Whether this group uses a grid icon (auto-generated from app icons).
        /// </summary>
        [JsonPropertyName("isGridIcon")]
        public bool IsGridIcon { get; set; } = false;

        /// <summary>
        /// Whether to show animations for this group.
        /// </summary>
        [JsonPropertyName("enableAnimations")]
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// Collection of applications/items in this group.
        /// Key: path or unique identifier, Value: AppItemConfig
        /// </summary>
        [JsonPropertyName("path")]
        public Dictionary<string, AppItemConfig> Items { get; set; } = new Dictionary<string, AppItemConfig>();

        /// <summary>
        /// Order of items in the group (for drag & drop persistence).
        /// </summary>
        [JsonPropertyName("itemOrder")]
        public List<string> ItemOrder { get; set; } = new List<string>();

        public GroupConfig() { }

        public GroupConfig(int id)
        {
            Id = id;
        }

        public GroupConfig(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the icon style (Regular or Grid).
        /// </summary>
        [JsonIgnore]
        public string IconStyle
        {
            get => IsGridIcon ? "Grid" : "Regular";
            set => IsGridIcon = value.Equals("Grid", System.StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is GroupConfig other && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return $"Group {Id}: {Name} ({Items.Count} items)";
        }
    }
}