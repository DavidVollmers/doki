using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal class InitCommand : Command
{
    private const string GitIgnoreContent = "\n.doki/\n";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private readonly ILogger _logger;

    public InitCommand(ILogger<InitCommand> logger) : base("init", "Set up your repository for documentation generation.")
    {
        _logger = logger;

        this.SetHandler(HandleCommandAsync);
    }

    private async Task<int> HandleCommandAsync(InvocationContext context)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var dokiConfigFile = new FileInfo(Path.Combine(currentDirectory, "doki.config.json"));
        if (dokiConfigFile.Exists)
        {
            _logger.LogError("A doki.config.json file already exists.");
            return -1;
        }

        var cancellationToken = context.GetCancellationToken();

        var version = GetType().Assembly.GetName().Version?.ToString(3);

        var dokiConfig = new DokiConfig
        {
            Schema = $"https://cdn.dvolper.dev/doki/{version}/doki.config.schema.json",
            Inputs = ["**/*.csproj"],
            Outputs =
            [
                new DokiConfig.DokiConfigOutput
                {
                    Type = "Doki.Output.Markdown"
                }
            ]
        };

        await File.WriteAllTextAsync(dokiConfigFile.FullName, JsonSerializer.Serialize(dokiConfig, SerializerOptions),
            cancellationToken);

        _logger.LogInformation("[bold green]Created doki.config.json file.[/]");

        var gitIgnoreFile = new FileInfo(Path.Combine(currentDirectory, ".gitignore"));
        if (gitIgnoreFile.Exists)
        {
            var gitIgnoreContent = await File.ReadAllTextAsync(gitIgnoreFile.FullName, cancellationToken);
            if (!gitIgnoreContent.Contains(".doki/"))
            {
                await File.AppendAllTextAsync(gitIgnoreFile.FullName, GitIgnoreContent, cancellationToken);
                _logger.LogInformation("[bold green]Updated .gitignore file.[/]");
            }
        }
        else
        {
            await File.WriteAllTextAsync(gitIgnoreFile.FullName, GitIgnoreContent, cancellationToken);
            _logger.LogInformation("[bold green]Created .gitignore file.[/]");
        }

        _logger.LogInformation("You can now run 'doki build' to generate documentation.");

        return 0;
    }
}