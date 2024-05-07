using System.Text.Json;

namespace Doki.Output.Json;

public sealed class JsonOutput(OutputOptions<JsonOutput> options) : IOutput
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        options.ClearOutputDirectoryIfRequired();

        return Task.CompletedTask;
    }

    public async Task WriteAsync(DocumentationRoot root, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(root);

        foreach (var assemblyDocumentation in root.Assemblies)
        {
            var file = new FileInfo(Path.Combine(options.OutputDirectory.FullName, $"{assemblyDocumentation.Id}.json"));

            if (!file.Directory!.Exists) file.Directory.Create();

            await File.WriteAllTextAsync(file.FullName,
                JsonSerializer.Serialize(assemblyDocumentation, JsonSerializerOptions), cancellationToken);
        }
    }
}