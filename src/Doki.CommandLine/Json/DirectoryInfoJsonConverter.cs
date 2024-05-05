using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doki.CommandLine.Json;

internal class DirectoryInfoJsonConverter(FileSystemInfo workingDirectory) : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var path = reader.GetString();

        if (string.IsNullOrWhiteSpace(path)) return null;

        return Path.IsPathRooted(path)
            ? new DirectoryInfo(path)
            : new DirectoryInfo(Path.Combine(workingDirectory.FullName, path));
    }

    public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.FullName);
    }
}