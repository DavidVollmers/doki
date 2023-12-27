using System.Text.Json;

namespace Doki.Output;

public sealed record OutputContext(DirectoryInfo WorkingDirectory, JsonElement? Options = null);