using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppGroup.Models
{
    /// <summary>
    /// Serializes <see cref="AppGroupConfig"/> in the "flat" on-disk format used by
    /// AppGroup: each group is a top-level property keyed by its string ID, alongside
    /// the "configVersion" and "lastModified" metadata properties.
    ///
    /// <para>
    /// This replaces a previous <c>[JsonExtensionData]</c> on a strongly-typed
    /// <c>Dictionary&lt;string, GroupConfig&gt;</c>, which is INVALID for
    /// System.Text.Json — it threw <see cref="InvalidOperationException"/> on every
    /// serialize/deserialize, silently breaking ConfigService (LoadConfig returned an
    /// empty config; SaveConfig wrote nothing).
    /// </para>
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
                reader.Read(); // advance to the value

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
                        var group = JsonSerializer.Deserialize<GroupConfig>(ref reader, options);
                        if (group != null)
                        {
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
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
