namespace Doki.Output;

/// <summary>
/// Default output options.
/// </summary>
public sealed record DefaultOutputOptions<T>(DirectoryInfo OutputDirectory) : IOutputOptions<T> where T : IOutput
{
}