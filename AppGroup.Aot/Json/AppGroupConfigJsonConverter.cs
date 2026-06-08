using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppGroup.Aot.Models;

namespace AppGroup.Aot.Json
{
    /// <summary>
    /// Custom converter that preserves the "flat" on-disk format shared with the WPF
    /// version of AppGroup: each group is a top-level property keyed by its string ID,
    /// alongside the "configVersion" and "lastModified" metadata properties.
    ///
    /// <para>
    /// Example:
    /// <code>
    /// {
    ///   "configVersion": "1.0",
    ///   "lastModified": "",
    ///   "1": { "groupName": "MesOutils", ... },
    ///   "2": { "groupName": "Dev", ... }
    /// }
    /// </code>
    /// </para>
    ///
    /// Individual groups are (de)serialized through the source-generated
    /// <see cref="AppJsonContext"/> so the whole path stays reflection-free (AOT-safe).
    /// </summary>
    public class AppGroupConfigJsonConverter : JsonConverter<AppGroupConfig>
    {
        public override AppGroupConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object for AppGroupConfig.");

            var config = new AppGroupConfig();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return config;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a property name.");

                string propertyName = reader.GetString()!;
                reader.Read(); // advance to the property value

                switch (propertyName)
                {
                    case "configVersion":
                        config.ConfigVersion = reader.GetString() ?? "1.0";
                        break;
                    case "lastModified":
                        config.LastModified = reader.GetString() ?? string.Empty;
                        break;
                    default:
                        // Any other property is a group, keyed by its string ID.
                        var group = JsonSerializer.Deserialize(ref reader, AppJsonContext.Default.GroupConfig);
                        if (group != null)
                        {
                            // Keep the in-memory Id consistent with the JSON key.
                            if (int.TryParse(propertyName, out int id))
                                group.Id = id;
                            config.Groups[propertyName] = group;
                        }
                        break;
                }
            }

            throw new JsonException("Unexpected end of JSON while reading AppGroupConfig.");
        }

        public override void Write(Utf8JsonWriter writer, AppGroupConfig value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("configVersion", value.ConfigVersion);
            writer.WriteString("lastModified", value.LastModified);

            foreach (var kvp in value.Groups)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, AppJsonContext.Default.GroupConfig);
            }

            writer.WriteEndObject();
        }
    }
}
