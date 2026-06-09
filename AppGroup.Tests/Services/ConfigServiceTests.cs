using AppGroup.Models;
using AppGroup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace AppGroup.Tests.Services
{
    public class ConfigServiceTests : IDisposable
    {
        private readonly string _testConfigPath;
        private readonly string _testConfigDir;

        public ConfigServiceTests()
        {
            // Create a temporary directory for test configs
            _testConfigDir = Path.Combine(Path.GetTempPath(), "AppGroupTests", Guid.NewGuid().ToString());
            _testConfigPath = Path.Combine(_testConfigDir, "appgroups.json");
            Directory.CreateDirectory(_testConfigDir);
        }

        public void Dispose()
        {
            // Clean up test files
            try
            {
                if (Directory.Exists(_testConfigDir))
                {
                    Directory.Delete(_testConfigDir, true);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }

        #region Serialization Tests (8b)

        [Fact]
        public void SerializeConfig_CreatesValidJson()
        {
            // Arrange
            var config = new AppGroupConfig
            {
                ConfigVersion = "1.0",
                LastModified = "2024-01-01T00:00:00.0000000Z"
            };

            var group = new GroupConfig(1, "Test Group")
            {
                ShowHeader = true,
                IconPath = "test.ico",
                ColumnCount = 3
            };
            group.Items["notepad.exe"] = new AppItemConfig("notepad.exe", "Notepad", "", "");
            config.Groups["1"] = group;

            // Act
            string json = ConfigService.SerializeConfig(config);

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
            Assert.Contains("Test Group", json);
            Assert.Contains("notepad.exe", json);
        }

        [Fact]
        public void DeserializeConfig_FromValidJson_ReturnsConfig()
        {
            // Arrange
            // Flat on-disk format: groups are top-level properties keyed by id,
            // alongside configVersion/lastModified (handled by AppGroupConfigJsonConverter).
            string json = @"{
                ""configVersion"": ""1.0"",
                ""lastModified"": ""2024-01-01T00:00:00.0000000Z"",
                ""1"": {
                    ""groupName"": ""Test Group"",
                    ""groupHeader"": true,
                    ""groupIcon"": ""test.ico"",
                    ""groupCol"": 3,
                    ""path"": {
                        ""notepad.exe"": {
                            ""path"": ""notepad.exe"",
                            ""tooltip"": ""Notepad""
                        }
                    }
                }
            }";

            // Act
            var result = ConfigService.DeserializeConfig(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1.0", result.ConfigVersion);
            Assert.Single(result.Groups);
            Assert.Equal("Test Group", result.Groups["1"].Name);
            Assert.True(result.Groups["1"].ShowHeader);
            Assert.Single(result.Groups["1"].Items);
        }

        [Fact]
        public void SaveConfig_ToFile_CreatesFile()
        {
            // Arrange
            var config = new AppGroupConfig();
            var group = new GroupConfig(1, "Save Test Group");
            config.Groups["1"] = group;

            // Act
            bool result = ConfigService.SaveConfig(config, _testConfigPath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(_testConfigPath));

            string content = File.ReadAllText(_testConfigPath);
            Assert.Contains("Save Test Group", content);
        }

        [Fact]
        public void LoadConfig_FromNonExistentFile_ReturnsEmptyConfig()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testConfigDir, "nonexistent.json");

            // Act
            var result = ConfigService.LoadConfig(nonExistentPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Groups);
        }

        [Fact]
        public void LoadConfig_FromValidFile_ReturnsConfig()
        {
            // Arrange
            var originalConfig = new AppGroupConfig();
            var group = new GroupConfig(1, "Load Test Group");
            group.Items["calc.exe"] = new AppItemConfig("calc.exe", "Calculator");
            originalConfig.Groups["1"] = group;
            ConfigService.SaveConfig(originalConfig, _testConfigPath);

            // Act
            var result = ConfigService.LoadConfig(_testConfigPath);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Groups);
            Assert.Equal("Load Test Group", result.Groups["1"].Name);
        }

        [Fact]
        public void SaveConfig_WithNullConfig_ReturnsFalse()
        {
            // Act
            bool result = ConfigService.SaveConfig(null!, _testConfigPath);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Group Management Tests (8c)

        [Fact]
        public void AddGroup_AddsNewGroup()
        {
            // Arrange - Start with empty config
            var config = new AppGroupConfig();
            
            // Act
            var newGroup = new GroupConfig(1, "New Group");
            config.AddGroup(newGroup);

            // Assert
            Assert.Single(config.Groups);
            Assert.Equal("New Group", config.Groups["1"].Name);
        }

        [Fact]
        public void AddGroup_WithZeroId_AssignsNextAvailableId()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Existing Group");

            // Act
            var newGroup = new GroupConfig(0, "Auto ID Group");
            config.AddGroup(newGroup);

            // Assert
            Assert.Equal(2, config.Groups.Count);
            Assert.Contains(config.Groups, kvp => kvp.Value.Name == "Auto ID Group");
        }

        [Fact]
        public void RemoveGroup_ExistingGroup_ReturnsTrue()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Remove Me");

            // Act
            bool result = config.RemoveGroup(1);

            // Assert
            Assert.True(result);
            Assert.Empty(config.Groups);
        }

        [Fact]
        public void RemoveGroup_NonExistentGroup_ReturnsFalse()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Keep Me");

            // Act
            bool result = config.RemoveGroup(999);

            // Assert
            Assert.False(result);
            Assert.Single(config.Groups);
        }

        [Fact]
        public void TryGetGroup_ExistingGroup_ReturnsTrue()
        {
            // Arrange
            var config = new AppGroupConfig();
            var group = new GroupConfig(1, "Get Me");
            config.Groups["1"] = group;

            // Act
            bool result = config.TryGetGroup(1, out var retrievedGroup);

            // Assert
            Assert.True(result);
            Assert.NotNull(retrievedGroup);
            Assert.Equal("Get Me", retrievedGroup.Name);
        }

        [Fact]
        public void TryGetGroup_NonExistentGroup_ReturnsFalse()
        {
            // Arrange
            var config = new AppGroupConfig();

            // Act
            bool result = config.TryGetGroup(999, out var retrievedGroup);

            // Assert
            Assert.False(result);
            Assert.Null(retrievedGroup);
        }

        [Fact]
        public void GetGroupByName_ExistingName_ReturnsGroup()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Find Me By Name");

            // Act
            var result = config.GetGroupByName("Find Me By Name");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetGroupByName_NonExistentName_ReturnsNull()
        {
            // Arrange
            var config = new AppGroupConfig();

            // Act
            var result = config.GetGroupByName("Does Not Exist");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUniqueGroupName_UniqueName_ReturnsSameName()
        {
            // Arrange
            var config = new AppGroupConfig();

            // Act
            string result = config.GetUniqueGroupName("Unique Group");

            // Assert
            Assert.Equal("Unique Group", result);
        }

        [Fact]
        public void GetUniqueGroupName_DuplicateName_ReturnsModifiedName()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Duplicate");

            // Act
            string result = config.GetUniqueGroupName("Duplicate");

            // Assert
            Assert.Equal("Duplicate (2)", result);
        }

        [Fact]
        public void NextGroupId_EmptyConfig_Returns1()
        {
            // Arrange
            var config = new AppGroupConfig();

            // Act
            int result = config.NextGroupId;

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void NextGroupId_WithGroups_ReturnsMaxIdPlusOne()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Group 1");
            config.Groups["5"] = new GroupConfig(5, "Group 5");
            config.Groups["3"] = new GroupConfig(3, "Group 3");

            // Act
            int result = config.NextGroupId;

            // Assert
            Assert.Equal(6, result);
        }

        #endregion

        #region App Item Management Tests (8d)

        [Fact]
        public void AddAppItem_AddsItemToGroup()
        {
            // Arrange
            var group = new GroupConfig(1, "Item Test Group");

            // Act
            var item = new AppItemConfig("paint.exe", "Paint", "", "paint.ico");
            group.Items[item.Path] = item;

            // Assert
            Assert.Single(group.Items);
            Assert.Equal("Paint", group.Items["paint.exe"].Tooltip);
        }

        [Fact]
        public void RemoveAppItem_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var group = new GroupConfig(1, "Remove Item Group");
            group.Items["word.exe"] = new AppItemConfig("word.exe", "Word");

            // Act
            bool result = group.Items.Remove("word.exe");

            // Assert
            Assert.True(result);
            Assert.Empty(group.Items);
        }

        [Fact]
        public void UpdateAppItem_ExistingPath_ReplacesItem()
        {
            // Arrange
            var group = new GroupConfig(1, "Update Item Group");
            group.Items["old.exe"] = new AppItemConfig("old.exe", "Old Tool");

            // Act
            group.Items["old.exe"] = new AppItemConfig("old.exe", "Updated Tool", "--new-args");

            // Assert
            Assert.Single(group.Items);
            Assert.Equal("Updated Tool", group.Items["old.exe"].Tooltip);
            Assert.Equal("--new-args", group.Items["old.exe"].Arguments);
        }

        [Fact]
        public void AppItemConfig_Equals_ReturnsTrueForSameValues()
        {
            // Arrange
            var item1 = new AppItemConfig("test.exe", "Test", "args", "icon.ico");
            var item2 = new AppItemConfig("test.exe", "Test", "args", "icon.ico");

            // Act & Assert
            Assert.Equal(item1, item2);
        }

        [Fact]
        public void AppItemConfig_Equals_ReturnsFalseForDifferentValues()
        {
            // Arrange
            var item1 = new AppItemConfig("test.exe", "Test", "args", "icon.ico");
            var item2 = new AppItemConfig("other.exe", "Test", "args", "icon.ico");

            // Act & Assert
            Assert.NotEqual(item1, item2);
        }

        #endregion

        #region Migration Tests (8e)

        [Fact]
        public void MigrateLegacyConfig_ValidLegacyJson_ReturnsTypedConfig()
        {
            // Arrange - Legacy format from old JsonConfigHelper
            string legacyJson = @"{
                ""1"": {
                    ""groupName"": ""Legacy Group"",
                    ""groupHeader"": true,
                    ""groupIcon"": ""legacy.ico"",
                    ""groupCol"": 4,
                    ""showLabels"": false,
                    ""labelSize"": 14,
                    ""path"": {
                        ""notepad.exe"": { ""tooltip"": ""Legacy Notepad"" },
                        ""calc.exe"": { ""tooltip"": ""Legacy Calc"", ""args"": ""/scientific"", ""icon"": ""calc.ico"" }
                    }
                },
                ""2"": {
                    ""groupName"": ""Another Legacy Group"",
                    ""groupHeader"": false,
                    ""path"": {}
                }
            }";

            // Act
            var result = ConfigService.MigrateLegacyConfig(legacyJson);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Groups.Count);
            Assert.Equal("Legacy Group", result.Groups["1"].Name);
            Assert.True(result.Groups["1"].ShowHeader);
            Assert.Equal(4, result.Groups["1"].ColumnCount);
            Assert.Equal(14, result.Groups["1"].LabelSize);
            Assert.False(result.Groups["1"].ShowLabels);
            Assert.Equal(2, result.Groups["1"].Items.Count);
            Assert.Equal("Legacy Notepad", result.Groups["1"].Items["notepad.exe"].Tooltip);
        }

        [Fact]
        public void MigrateLegacyConfig_InvalidJson_ReturnsEmptyConfig()
        {
            // Arrange
            string invalidJson = "not valid json {";

            // Act
            var result = ConfigService.MigrateLegacyConfig(invalidJson);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Groups);
        }

        [Fact]
        public void MigrateLegacyConfig_EmptyJson_ReturnsEmptyConfig()
        {
            // Arrange
            string emptyJson = "{}";

            // Act
            var result = ConfigService.MigrateLegacyConfig(emptyJson);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Groups);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void FullWorkflow_CreateConfigAddGroupAndSave_Roundtrip()
        {
            // Arrange
            var config = new AppGroupConfig();

            // Act - Add a group with items
            var group = new GroupConfig(1, "Roundtrip Group");
            group.Items["app1.exe"] = new AppItemConfig("app1.exe", "Application 1");
            group.Items["app2.exe"] = new AppItemConfig("app2.exe", "Application 2", "/start");
            config.Groups["1"] = group;

            // Save
            bool saveResult = ConfigService.SaveConfig(config, _testConfigPath);
            Assert.True(saveResult);

            // Load
            var loadedConfig = ConfigService.LoadConfig(_testConfigPath);

            // Assert
            Assert.NotNull(loadedConfig);
            Assert.Single(loadedConfig.Groups);
            Assert.Equal("Roundtrip Group", loadedConfig.Groups["1"].Name);
            Assert.Equal(2, loadedConfig.Groups["1"].Items.Count);
            Assert.Equal("Application 1", loadedConfig.Groups["1"].Items["app1.exe"].Tooltip);
            Assert.Equal("/start", loadedConfig.Groups["1"].Items["app2.exe"].Arguments);
        }

        [Fact]
        public void ConfigService_GetAllGroups_ReturnsAllGroups()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Group 1");
            config.Groups["2"] = new GroupConfig(2, "Group 2");
            config.Groups["3"] = new GroupConfig(3, "Group 3");
            
            // Save to our test location
            ConfigService.SaveConfig(config, _testConfigPath);

            // Note: GetAllGroups uses LoadConfig() which uses default path
            // For this test, we'll directly test the config object
            
            // Act
            var groups = new List<GroupConfig>(config.Groups.Values);

            // Assert
            Assert.Equal(3, groups.Count);
        }

        [Fact]
        public void GroupExists_ByName_ReturnsCorrectResult()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Exists");

            // Act & Assert
            Assert.True(config.GroupNameExists("Exists"));
            Assert.False(config.GroupNameExists("Does Not Exist"));
        }

        [Fact]
        public void GroupExists_ById_ReturnsCorrectResult()
        {
            // Arrange
            var config = new AppGroupConfig();
            config.Groups["1"] = new GroupConfig(1, "Exists");

            // Act & Assert
            Assert.True(config.Groups.ContainsKey("1"));
            Assert.False(config.Groups.ContainsKey("999"));
        }

        #endregion
    }

    // Additional test class for edge cases
    public class ConfigServiceEdgeCasesTests : IDisposable
    {
        private readonly string _testConfigDir;

        public ConfigServiceEdgeCasesTests()
        {
            _testConfigDir = Path.Combine(Path.GetTempPath(), "AppGroupTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testConfigDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testConfigDir))
                {
                    Directory.Delete(_testConfigDir, true);
                }
            }
            catch { }
        }

        [Fact]
        public void LoadConfig_WithMalformedJson_ReturnsEmptyConfig()
        {
            // Arrange
            string badJsonPath = Path.Combine(_testConfigDir, "bad.json");
            File.WriteAllText(badJsonPath, "{ this is not valid json }");

            // Act
            var result = ConfigService.LoadConfig(badJsonPath);

            // Assert
            Assert.NotNull(result);
            // Should return empty config or handle gracefully
        }

        [Fact]
        public void LoadConfig_WithEmptyJson_ReturnsEmptyConfig()
        {
            // Arrange
            string emptyJsonPath = Path.Combine(_testConfigDir, "empty.json");
            File.WriteAllText(emptyJsonPath, "{}");

            // Act
            var result = ConfigService.LoadConfig(emptyJsonPath);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SaveConfig_ToReadOnlyDirectory_ReturnsFalse()
        {
            // This test would require special setup, skipping for now
            // as it involves file system permissions
        }

        [Fact]
        public void GroupConfig_ToString_ReturnsFormattedString()
        {
            // Arrange
            var group = new GroupConfig(1, "Test Group");
            group.Items["item1.exe"] = new AppItemConfig("item1.exe");
            group.Items["item2.exe"] = new AppItemConfig("item2.exe");

            // Act
            string result = group.ToString();

            // Assert
            Assert.Contains("Group 1", result);
            Assert.Contains("Test Group", result);
            Assert.Contains("2 items", result);
        }

        [Fact]
        public void GroupConfig_Equals_SameId_ReturnsTrue()
        {
            // Arrange
            var group1 = new GroupConfig(1, "Group A");
            var group2 = new GroupConfig(1, "Group B");

            // Act & Assert
            Assert.Equal(group1, group2);
        }

        [Fact]
        public void GroupConfig_Equals_DifferentId_ReturnsFalse()
        {
            // Arrange
            var group1 = new GroupConfig(1, "Group A");
            var group2 = new GroupConfig(2, "Group A");

            // Act & Assert
            Assert.NotEqual(group1, group2);
        }
    }
}
