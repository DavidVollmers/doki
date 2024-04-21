using System.Text.Json;

namespace Doki.Output;

/// <summary>
/// Base class for file based outputs.
/// </summary>
/// <typeparam name="T">The type of options for the output.</typeparam>
public abstract class OutputBase<T> : IOutput where T : OutputOptions, new()
{
    private readonly DirectoryInfo _outputDirectory;

    /// <summary>
    /// Gets the options for the output.
    /// </summary>
    protected T? Options { get; }

    /// <summary>
    /// Gets the output context.
    /// </summary>
    protected OutputContext Context { get; }

    /// <summary>
    /// Gets the output directory.
    /// </summary>
    /// <remarks>
    /// If the directory does not exist, it will be created.
    /// </remarks>
    protected DirectoryInfo OutputDirectory
    {
        get
        {
            if (!_outputDirectory.Exists) _outputDirectory.Create();
            return _outputDirectory;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputBase{T}"/> class.
    /// </summary>
    /// <param name="context">The output context.</param>
    protected OutputBase(OutputContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;

        if (context.Options != null) Options = JsonSerializer.Deserialize<T>(context.Options.Value.GetRawText());

        _outputDirectory = new DirectoryInfo(Path.Combine(Context.WorkingDirectory.FullName,
            Options?.OutputPath ?? OutputOptions.Default.OutputPath!));
    }

    /// <summary>
    /// Writes the content list to the output.
    /// </summary>
    /// <param name="contentList">The content list to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the type documentation to the output.
    /// </summary>
    /// <param name="typeDocumentation">The type documentation to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default);
}