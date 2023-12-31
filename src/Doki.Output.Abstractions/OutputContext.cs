using System.Text.Json;

namespace Doki.Output;

/// <summary>
/// The context in which the output is being generated.
/// </summary>
/// <param name="WorkingDirectory">The directory in which the output is being generated.</param>
/// <param name="Options">The JSON serialized options to use when generating the output.</param>
public sealed record OutputContext(DirectoryInfo WorkingDirectory, JsonElement? Options = null);