using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal class InitCommand : Command
{
    private readonly ILogger _logger;

    public InitCommand(ILogger<InitCommand> logger) : base("init", "Initialize the Doki configuration file.")
    {
        _logger = logger;

        this.SetHandler(HandleCommandAsync);
    }

    private async Task<int> HandleCommandAsync(InvocationContext context)
    {
        var dokiConfigFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "doki.config.json"));

        if (dokiConfigFile.Exists)
        {
            _logger.LogError("A doki.config.json file already exists.");
            return -1;
        }

        var cancellationToken = context.GetCancellationToken();

        var version = GetType().Assembly.GetName().Version?.ToString();

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

        await File.WriteAllTextAsync(dokiConfigFile.FullName,
            JsonSerializer.Serialize(dokiConfig, new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken);

        _logger.LogInformation("[bold green]Created doki.config.json file.[/]");
        
        //TODO add or create .doki folder .gitignore entry

        _logger.LogInformation("You can now run 'doki build' to generate documentation.");

        return 0;
    }
}