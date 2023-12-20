using System.Text.Json;

namespace Doki.Output;

public abstract class OutputBase<T> : IOutput where T : OutputOptions
{
    private readonly DirectoryInfo _outputDirectory;

    protected T? Options { get; }

    protected OutputContext Context { get; }

    protected DirectoryInfo OutputDirectory
    {
        get
        {
            if (!_outputDirectory.Exists) _outputDirectory.Create();
            return _outputDirectory;
        }
    }

    protected OutputBase(OutputContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;

        if (context.Options != null) Options = JsonSerializer.Deserialize<T>(context.Options.Value.GetRawText());

        _outputDirectory = new DirectoryInfo(Path.Combine(Context.ProjectDirectory.FullName,
            Options?.OutputPath ?? OutputOptions.Default.OutputPath!));
    }

    public abstract Task WriteAsync(TableOfContents tableOfContents, CancellationToken cancellationToken = default);
}