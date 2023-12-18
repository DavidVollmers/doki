using System.Text.Json;

namespace Doki.Output;

public sealed record OutputContext(DirectoryInfo ProjectDirectory, JsonElement? Options);