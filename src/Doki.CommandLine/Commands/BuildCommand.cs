using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Xml.XPath;
using Doki.Output;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Doki.CommandLine.Commands;

internal class BuildCommand : Command
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Argument<FileInfo?> _targetArgument =
        new("CONFIG", "The doki.config.json file to use for documentation generation.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly Option<string> _buildConfigurationOption =
        new(new[] {"-c", "--configuration"}, "The build configuration to use when building projects.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly ILogger _logger;

    public BuildCommand(ILogger<BuildCommand> logger) : base("build", "Builds documentation for a project.")
    {
        _logger = logger;

        AddArgument(_targetArgument);

        AddOption(_buildConfigurationOption);

        this.SetHandler(HandleCommandAsync);
    }

    private async Task HandleCommandAsync(InvocationContext context)
    {
        var dokiConfigFile = context.ParseResult.GetValueForArgument(_targetArgument);
        var buildConfiguration = context.ParseResult.GetValueForOption(_buildConfigurationOption) ?? "Release";

        if (dokiConfigFile == null)
        {
            dokiConfigFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "doki.config.json"));

            if (!dokiConfigFile.Exists)
            {
                _logger.LogError("Could not find doki.config.json file.");
                return;
            }
        }

        var cancellationToken = context.GetCancellationToken();

        var generator = new DocumentationGenerator();

        await ConfigureDocumentationGeneratorAsync(generator, dokiConfigFile, buildConfiguration, cancellationToken);

        await generator.GenerateAsync(_logger, cancellationToken);

        _logger.LogInformation("[bold green]Documentation generated.[/]");
    }

    //TODO return int for error handling
    private async Task ConfigureDocumentationGeneratorAsync(DocumentationGenerator generator,
        FileInfo dokiConfigFile, string buildConfiguration, CancellationToken cancellationToken)
    {
        var dokiConfig = await JsonSerializer.DeserializeAsync<DokiConfig>(dokiConfigFile.OpenRead(),
            JsonSerializerOptions, cancellationToken);

        if (dokiConfig == null)
        {
            _logger.LogError("Could not deserialize doki config file.");
            return;
        }

        await LoadInputsAsync(generator, dokiConfigFile.Directory!, dokiConfig.Inputs, buildConfiguration,
            cancellationToken);

        await LoadOutputsAsync(generator, dokiConfigFile.Directory!, dokiConfig.Outputs, cancellationToken);
    }

    private async Task LoadInputsAsync(DocumentationGenerator generator, DirectoryInfo workingDirectory,
        string[]? inputs, string buildConfiguration, CancellationToken cancellationToken)
    {
        if (inputs == null)
        {
            _logger.LogError("No inputs configured.");
            return;
        }

        var matcher = new Matcher();
        foreach (var input in inputs)
        {
            if (input.StartsWith('!')) matcher.AddExclude(input[1..]);
            else matcher.AddInclude(input);
        }

        var result = matcher.Execute(new DirectoryInfoWrapper(workingDirectory));
        if (!result.HasMatches)
        {
            _logger.LogError("No inputs matched.");
            return;
        }

        foreach (var file in result.Files)
        {
            var projectFile = new FileInfo(Path.Combine(workingDirectory.FullName, file.Path));

            var navigator = new XPathDocument(projectFile.FullName).CreateNavigator();

            var targetFrameworks = navigator.SelectSingleNode("/Project/PropertyGroup/TargetFramework")?.Value ??
                                   navigator.SelectSingleNode("/Project/PropertyGroup/TargetFrameworks")?.Value;
            if (targetFrameworks == null)
            {
                _logger.LogError("Could not determine target framework.");
                return;
            }

            await BuildProjectAsync(projectFile, buildConfiguration, true, cancellationToken);

            var latestTargetFramework = targetFrameworks.Split(';').OrderByDescending(x => x).First();

            var projectName = projectFile.Name[..^projectFile.Extension.Length];

            var assemblyPath = Path.Combine(projectFile.DirectoryName!, "bin", buildConfiguration,
                latestTargetFramework, $"{projectName}.dll");
            var loadContext = new DokiAssemblyLoadContext(assemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

            var documentationFile = new XPathDocument(Path.Combine(projectFile.DirectoryName!, "bin",
                buildConfiguration, latestTargetFramework, $"{projectName}.xml"));

            generator.AddAssembly(assembly, documentationFile);
        }
    }

    private async Task LoadOutputsAsync(DocumentationGenerator generator, DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput[]? outputs, CancellationToken cancellationToken)
    {
        if (outputs == null)
        {
            _logger.LogError("No outputs configured.");
            return;
        }

        foreach (var output in outputs)
        {
            if (output.From == null)
            {
                _logger.LogError("No from configured.");
                return;
            }

            if (output.Type == null)
            {
                _logger.LogError("No type configured.");
                return;
            }

            var instance = await LoadOutputAsync(workingDirectory, output, cancellationToken);

            if (instance == null)
            {
                _logger.LogError("Could not load output.");
                return;
            }

            generator.AddOutput(instance);
        }
    }

    private async Task<IOutput?> LoadOutputAsync(DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, CancellationToken cancellationToken)
    {
        var outputContext = new OutputContext(workingDirectory, output.Options);

        if (output.From!.EndsWith(".csproj"))
        {
            var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From));

            await BuildProjectAsync(fileInfo, "Release", false, cancellationToken);

            var assembly = Assembly.LoadFrom(Path.Combine(fileInfo.DirectoryName!, "bin", "Release", "net8.0",
                $"{fileInfo.Name[..^fileInfo.Extension.Length]}.dll"));

            var outputType = assembly.GetType(output.Type!) ?? assembly
                .GetExportedTypes()
                .FirstOrDefault(t => t.GetCustomAttribute<DokiOutputAttribute>()?.Name == output.Type);

            if (outputType == null) return null;

            return Activator.CreateInstance(outputType, outputContext) as IOutput;
        }

        return null;
    }

    private async Task BuildProjectAsync(FileSystemInfo projectFile, string buildConfiguration,
        bool buildForDoki, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Building project {projectFile.Name}...");

        var arguments = $"build \"{projectFile.FullName}\" -c {buildConfiguration}";
        if (buildForDoki) arguments += " /p:CopyLocalLockFileAssemblies=true /p:GenerateDocumentationFile=true";

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });

        if (process == null)
        {
            _logger.LogError("Could not start dotnet process.");
            return;
        }

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Build failed.");
            _logger.LogError(output);
            return;
        }

        _logger.LogInformation("[bold green]Build succeeded.[/]");
    }
}