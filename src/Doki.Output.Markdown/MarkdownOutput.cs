using System.Text.Json;
using Doki.Elements;

namespace Doki.Output.Markdown;

[DokiOutput("Doki.Output.Markdown")]
public sealed class MarkdownOutput : IOutput
{
    private readonly OutputOptions _options;

    public MarkdownOutput(JsonElement? options)
    {
        if (options == null) _options = OutputOptions.Default;
        else _options = JsonSerializer.Deserialize<OutputOptions>(options.Value.GetRawText()) ?? OutputOptions.Default;
    }

    public async Task WriteAsync(TableOfContents tableOfContents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableOfContents);

        if (_options.OutputDirectory == null) throw new InvalidOperationException("Output directory not set.");

        var outputDirectoryInfo = new DirectoryInfo(_options.OutputDirectory);

        if (!outputDirectoryInfo.Exists) outputDirectoryInfo.Create();

        var tableOfContentsFile =
            new FileInfo(Path.Combine(outputDirectoryInfo.FullName, $"{tableOfContents.Name}.md"));

        var markdown = "TODO";
        
        await File.WriteAllTextAsync(tableOfContentsFile.FullName, markdown, cancellationToken).ConfigureAwait(false);

        foreach (var child in tableOfContents.Children)
        {
            await WriteAsync(child, cancellationToken).ConfigureAwait(false);
        }
    }
}