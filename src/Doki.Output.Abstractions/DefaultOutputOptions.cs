namespace Doki.Output;

/// <summary>
/// Default output options.
/// </summary>
public sealed record DefaultOutputOptions<T> : IOutputOptions<T> where T : IOutput
{
    /// <inheritdoc />
    public DirectoryInfo OutputDirectory { get; }

    internal DefaultOutputOptions(DirectoryInfo outputDirectory)
    {
        OutputDirectory = outputDirectory;
    }
}