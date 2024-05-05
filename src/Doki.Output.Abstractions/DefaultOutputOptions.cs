namespace Doki.Output;

/// <summary>
/// Default output options.
/// </summary>
public sealed record DefaultOutputOptions<T> : IOutputOptions<T> where T : IOutput
{
    public DirectoryInfo OutputDirectory { get; init; } = null!;
}