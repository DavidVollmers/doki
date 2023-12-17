using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Xml.XPath;
using Doki.Output;
using Spectre.Console;

namespace Doki.CommandLine.Commands;

internal class BuildCommand : Command
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Argument<FileInfo?> _projectArgument = new("PROJECT", "The project to build.")
    {
        Arity = ArgumentArity.ZeroOrOne
    };

    private readonly Option<string> _buildConfigurationOption =
        new(new[] { "-c", "--configuration" }, "The build configuration to use.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly Option<FileInfo?> _dokiConfigOption =
        new(new[] { "-d", "--doki-config" }, "The doki config file to use.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    public BuildCommand() : base("build", "Builds documentation for a project.")
    {
        AddArgument(_projectArgument);

        AddOption(_dokiConfigOption);
        AddOption(_buildConfigurationOption);

        this.SetHandler(HandleCommandAsync);
    }

    private async Task HandleCommandAsync(InvocationContext context)
    {
        var projectFile = context.ParseResult.GetValueForArgument(_projectArgument);
        var dokiConfigFile = context.ParseResult.GetValueForOption(_dokiConfigOption);
        var buildConfiguration = context.ParseResult.GetValueForOption(_buildConfigurationOption) ?? "Release";

        if (projectFile == null)
        {
            var projectFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");

            switch (projectFiles.Length)
            {
                case 0:
                    AnsiConsole.MarkupLine("[bold red]No project file found.[/]");
                    return;
                case > 1:
                    AnsiConsole.MarkupLine("[bold red]Multiple project files found.[/]");
                    return;
                default:
                    projectFile = new FileInfo(projectFiles[0]);
                    break;
            }
        }

        if (dokiConfigFile == null)
        {
            dokiConfigFile = new FileInfo(Path.Combine(projectFile.DirectoryName!, "doki.config.json"));

            if (!dokiConfigFile.Exists) throw new FileNotFoundException("Could not find doki config file.");
        }

        var navigator = new XPathDocument(projectFile.FullName).CreateNavigator();
        if (navigator.SelectSingleNode("/Project/PropertyGroup/GenerateDocumentationFile")?.Value != "true")
        {
            AnsiConsole.MarkupLine("[bold red]Documentation generation is not enabled for this project.[/]");
            return;
        }

        var targetFrameworks = navigator.SelectSingleNode("/Project/PropertyGroup/TargetFramework")?.Value ??
                               navigator.SelectSingleNode("/Project/PropertyGroup/TargetFrameworks")?.Value;
        if (targetFrameworks == null)
        {
            AnsiConsole.MarkupLine("[bold red]Could not determine target framework.[/]");
            return;
        }

        var cancellationToken = context.GetCancellationToken();

        await BuildProjectAsync(projectFile, buildConfiguration, cancellationToken);

        var latestTargetFramework = targetFrameworks.Split(';').OrderByDescending(x => x).First();

        var projectName = projectFile.Name[..^projectFile.Extension.Length];
        var assembly = Assembly.LoadFile(Path.Combine(projectFile.DirectoryName!, "bin", buildConfiguration,
            latestTargetFramework, $"{projectName}.dll"));
        var documentationFile = new XPathDocument(Path.Combine(projectFile.DirectoryName!, "bin", buildConfiguration,
            latestTargetFramework, $"{projectName}.xml"));

        var generator = new DocumentationGenerator(assembly, documentationFile);

        await ConfigureDocumentationGeneratorAsync(generator, projectFile.Directory!, dokiConfigFile,
            cancellationToken);

        AnsiConsole.MarkupLine($"Generating documentation for project {projectFile.Name}...");

        await generator.GenerateAsync(cancellationToken);
    }

    private static async Task ConfigureDocumentationGeneratorAsync(DocumentationGenerator generator,
        FileSystemInfo projectDirectory, FileInfo dokiConfigFile, CancellationToken cancellationToken)
    {
        var dokiConfig = await JsonSerializer.DeserializeAsync<DokiConfig>(dokiConfigFile.OpenRead(),
            JsonSerializerOptions, cancellationToken);

        if (dokiConfig == null)
        {
            AnsiConsole.MarkupLine("[bold red]Could not deserialize doki config file.[/]");
            return;
        }

        if (dokiConfig.Outputs == null)
        {
            AnsiConsole.MarkupLine("[bold red]No outputs configured.[/]");
            return;
        }

        foreach (var output in dokiConfig.Outputs)
        {
            if (output.From == null)
            {
                AnsiConsole.MarkupLine("[bold red]No from configured.[/]");
                return;
            }

            if (output.Type == null)
            {
                AnsiConsole.MarkupLine("[bold red]No type configured.[/]");
                return;
            }

            var instance = await LoadOutputAsync(projectDirectory, output, cancellationToken);

            if (instance == null)
            {
                AnsiConsole.MarkupLine("[bold red]Could not load output.[/]");
                return;
            }

            generator.AddOutput(instance);
        }
    }

    private static async Task<IOutput?> LoadOutputAsync(FileSystemInfo projectDirectory,
        DokiConfig.DokiConfigOutput output, CancellationToken cancellationToken)
    {
        if (output.From!.EndsWith(".csproj"))
        {
            var fileInfo = new FileInfo(Path.Combine(projectDirectory.FullName, output.From));

            await BuildProjectAsync(fileInfo, "Release", cancellationToken);

            var assembly = Assembly.LoadFrom(Path.Combine(fileInfo.DirectoryName!, "bin", "Release", "net8.0",
                $"{fileInfo.Name[..^fileInfo.Extension.Length]}.dll"));

            var outputType = assembly.GetType(output.Type!) ?? assembly
                .GetExportedTypes()
                .FirstOrDefault(t => t.GetCustomAttribute<DokiOutputAttribute>()?.Name == output.Type);

            if (outputType == null) return null;

            return Activator.CreateInstance(outputType, output.Options) as IOutput;
        }

        return null;
    }

    private static async Task BuildProjectAsync(FileSystemInfo projectFile, string buildConfiguration,
        CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"Building project {projectFile.Name}...");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectFile.FullName}\" -c {buildConfiguration}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });

        if (process == null)
        {
            AnsiConsole.MarkupLine("[bold red]Could not start dotnet process.[/]");
            return;
        }

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            AnsiConsole.MarkupLine("[bold red]Build failed.[/]");
            AnsiConsole.MarkupLine(output);
            return;
        }

        AnsiConsole.MarkupLine("[bold green]Build succeeded.[/]");
    }
}