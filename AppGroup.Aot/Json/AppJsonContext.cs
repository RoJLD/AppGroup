using System.Text.Json.Serialization;
using AppGroup.Aot.Models;

namespace AppGroup.Aot.Json
{
    /// <summary>
    /// Source-generated JSON context for Native AOT.
    /// In AOT, reflection-based serialization is disabled, so every type that needs
    /// to be (de)serialized must be declared here. The C# source generator emits the
    /// (de)serialization code at compile time — no reflection, fully trim-safe.
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(AppGroupConfig))]
    [JsonSerializable(typeof(GroupConfig))]
    [JsonSerializable(typeof(AppItemConfig))]
    internal partial class AppJsonContext : JsonSerializerContext
    {
    }
}
