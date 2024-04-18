using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.XPath;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal partial class BuildCommand : Command
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
        new(["-c", "--configuration"], "The build configuration to use when building projects.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly List<DokiAssemblyLoadContext> _loadContexts = [];
    private readonly List<string> _builtProjects = [];
    private readonly ILogger _logger;

    public BuildCommand(ILogger<BuildCommand> logger) : base("build", "Builds documentation for a project.")
    {
        _logger = logger;

        AddArgument(_targetArgument);

        AddOption(_buildConfigurationOption);

        this.SetHandler(HandleCommandAsync);
    }

    private async Task<int> HandleCommandAsync(InvocationContext context)
    {
        var dokiConfigFile = context.ParseResult.GetValueForArgument(_targetArgument);
        var buildConfiguration = context.ParseResult.GetValueForOption(_buildConfigurationOption) ?? "Release";

        if (dokiConfigFile == null)
        {
            dokiConfigFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "doki.config.json"));

            if (!dokiConfigFile.Exists)
            {
                _logger.LogError("Could not find doki.config.json file.");
                return -1;
            }
        }

        var cancellationToken = context.GetCancellationToken();

        var generator = new DocumentationGenerator();

        var configureResult =
            await ConfigureDocumentationGeneratorAsync(generator, dokiConfigFile, buildConfiguration,
                cancellationToken);
        if (configureResult != 0) return configureResult;

        await generator.GenerateAsync(_logger, cancellationToken);

        _logger.LogInformation("[bold green]Documentation generated.[/]");

        foreach (var loadContext in _loadContexts)
        {
            loadContext.Unload();
        }

        return 0;
    }

    private async Task<int> ConfigureDocumentationGeneratorAsync(DocumentationGenerator generator,
        FileInfo dokiConfigFile, string buildConfiguration, CancellationToken cancellationToken)
    {
        var dokiConfig = await JsonSerializer.DeserializeAsync<DokiConfig>(dokiConfigFile.OpenRead(),
            JsonSerializerOptions, cancellationToken);

        if (dokiConfig == null)
        {
            _logger.LogError("Could not deserialize doki config file.");
            return -1;
        }

        generator.IncludeInheritedMembers = dokiConfig.IncludeInheritedMembers;

        var inputResult = await LoadInputsAsync(generator, dokiConfigFile.Directory!, dokiConfig.Inputs,
            buildConfiguration, cancellationToken);
        if (inputResult != 0) return inputResult;

        var outputResult =
            await LoadOutputsAsync(generator, dokiConfigFile.Directory!, dokiConfig.Outputs, cancellationToken);
        if (outputResult != 0) return outputResult;

        var filterResult = CompileFilters(generator, dokiConfig.Filter);
        return filterResult != 0 ? filterResult : 0;
    }

    private async Task<int> LoadInputsAsync(DocumentationGenerator generator, DirectoryInfo workingDirectory,
        string[]? inputs, string buildConfiguration, CancellationToken cancellationToken)
    {
        if (inputs == null)
        {
            _logger.LogError("No inputs configured.");
            return -1;
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
            return -1;
        }

        foreach (var file in result.Files)
        {
            var projectFile = new FileInfo(Path.Combine(workingDirectory.FullName, file.Path));

            var projectMetadata = new XPathDocument(projectFile.FullName);

            var navigator = projectMetadata.CreateNavigator();

            var targetFrameworks = navigator.SelectSingleNode("/Project/PropertyGroup/TargetFramework")?.Value ??
                                   navigator.SelectSingleNode("/Project/PropertyGroup/TargetFrameworks")?.Value;
            if (targetFrameworks == null)
            {
                _logger.LogError("Could not determine target framework.");
                return -1;
            }

            var buildResult = await BuildProjectAsync(projectFile, buildConfiguration, true, cancellationToken);
            if (buildResult != 0) return buildResult;

            var latestTargetFramework = targetFrameworks.Split(';').OrderByDescending(x => x).First();

            var projectName = projectFile.Name[..^projectFile.Extension.Length];

            var assemblyPath = Path.Combine(projectFile.DirectoryName!, "bin", buildConfiguration,
                latestTargetFramework, $"{projectName}.dll");
            var loadContext = new DokiAssemblyLoadContext(assemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

            var documentationFile = new XPathDocument(Path.Combine(projectFile.DirectoryName!, "bin",
                buildConfiguration, latestTargetFramework, $"{projectName}.xml"));

            generator.AddAssembly(assembly, documentationFile, projectMetadata);

            _loadContexts.Add(loadContext);
        }

        return 0;
    }

    private async Task<int> LoadOutputsAsync(DocumentationGenerator generator, DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput[]? outputs, CancellationToken cancellationToken)
    {
        if (outputs == null)
        {
            _logger.LogError("No outputs configured.");
            return -1;
        }

        foreach (var output in outputs)
        {
            if (string.IsNullOrWhiteSpace(output.Type))
            {
                _logger.LogError("No type configured.");
                return -1;
            }

            var instance = await LoadOutputAsync(workingDirectory, output, cancellationToken);

            if (instance == null)
            {
                _logger.LogError("Could not load output.");
                return -1;
            }

            generator.AddOutput(instance);
        }

        return 0;
    }

    private async Task<int> BuildProjectAsync(FileSystemInfo projectFile, string buildConfiguration,
        bool buildForDoki, CancellationToken cancellationToken)
    {
        if (_builtProjects.Contains(projectFile.FullName))
        {
            _logger.LogDebug("Project already built: {ProjectFileName}", projectFile.Name);
            return 0;
        }

        _logger.LogInformation("Building project {ProjectFileName}...", projectFile.Name);

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
            return -1;
        }

        var output = await process.StandardOutput.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Build failed.");
            _logger.LogError(output);
            return -1;
        }

        _logger.LogInformation("[bold green]Build succeeded.[/]");

        //TODO add project dependencies
        _builtProjects.Add(projectFile.FullName);

        return 0;
    }
}