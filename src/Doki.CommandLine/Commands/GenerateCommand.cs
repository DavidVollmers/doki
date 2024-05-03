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

namespace Doki.CommandLine.Commands;

internal partial class GenerateCommand : Command
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Argument<FileInfo?> _targetArgument =
        new("CONFIG",
            "The path to the configuration file to use for documentation generation. If not specified, the command will look for a `doki.config.json` file in the current directory.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly Option<bool> _allowPreviewOption =
        new("--allow-preview",
            "Allow preview versions of the configured output libraries to be used during documentation generation.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly Option<string> _buildConfigurationOption =
        new(["-c", "--configuration"],
            "Defines the build configuration for building projects. If not specified, the command will use the \"Release\" configuration.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly Option<bool> _noBuildOption =
        new("--no-build", "Skip building the project/s before generating documentation.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private readonly List<DokiAssemblyLoadContext> _loadContexts = [];
    private readonly List<string> _builtProjects = [];
    private readonly ILogger _logger;

    public GenerateCommand(ILogger<GenerateCommand> logger) : base("g",
        "Generate documentation for your .NET projects.")
    {
        _logger = logger;

        AddArgument(_targetArgument);

        AddOption(_allowPreviewOption);
        AddOption(_buildConfigurationOption);
        AddOption(_noBuildOption);

        this.SetHandler(HandleCommandAsync);
    }

    private async Task<int> HandleCommandAsync(InvocationContext context)
    {
        var dokiConfigFile = context.ParseResult.GetValueForArgument(_targetArgument);
        var allowPreview = context.ParseResult.GetValueForOption(_allowPreviewOption);
        var buildConfiguration = context.ParseResult.GetValueForOption(_buildConfigurationOption) ?? "Release";
        var noBuild = context.ParseResult.GetValueForOption(_noBuildOption);

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

        var dokiConfig = await JsonSerializer.DeserializeAsync<DokiConfig>(dokiConfigFile.OpenRead(),
            JsonSerializerOptions, cancellationToken);

        if (dokiConfig == null)
        {
            _logger.LogError("Could not deserialize doki config file.");
            return -1;
        }

        var generator = new DocumentationGenerator();

        var configureResult = await ConfigureDocumentationGeneratorAsync(new GenerateContext
        {
            Generator = generator,
            DokiConfig = dokiConfig,
            AllowPreview = allowPreview,
            BuildConfiguration = buildConfiguration,
            NoBuild = noBuild,
            Directory = dokiConfigFile.Directory!
        }, cancellationToken);
        if (configureResult != 0) return configureResult;

        await generator.GenerateAsync(_logger, cancellationToken);

        _logger.LogInformation("[bold green]Documentation generated.[/]");

        foreach (var loadContext in _loadContexts)
        {
            loadContext.Unload();
        }

        return 0;
    }

    private async Task<int> ConfigureDocumentationGeneratorAsync(GenerateContext context,
        CancellationToken cancellationToken)
    {
        context.Generator.IncludeInheritedMembers = context.DokiConfig.IncludeInheritedMembers;

        var inputResult = await LoadInputsAsync(context, cancellationToken);
        if (inputResult != 0) return inputResult;

        var outputResult = await LoadOutputsAsync(context, cancellationToken);
        if (outputResult != 0) return outputResult;

        var filterResult = CompileFilters(context);
        return filterResult != 0 ? filterResult : 0;
    }

    private async Task<int> LoadInputsAsync(GenerateContext context, CancellationToken cancellationToken)
    {
        if (context.DokiConfig.Inputs == null)
        {
            _logger.LogError("No inputs configured.");
            return -1;
        }

        var matcher = new Matcher();
        foreach (var input in context.DokiConfig.Inputs)
        {
            if (input.StartsWith('!')) matcher.AddExclude(input[1..]);
            else matcher.AddInclude(input);
        }

        var result = matcher.Execute(new DirectoryInfoWrapper(context.Directory));
        if (!result.HasMatches)
        {
            _logger.LogError("No inputs matched.");
            return -1;
        }

        foreach (var file in result.Files)
        {
            var projectFile = new FileInfo(Path.Combine(context.Directory.FullName, file.Path));

            var projectMetadata = new XPathDocument(projectFile.FullName);

            var navigator = projectMetadata.CreateNavigator();

            var targetFrameworks = navigator.SelectSingleNode("/Project/PropertyGroup/TargetFramework")?.Value ??
                                   navigator.SelectSingleNode("/Project/PropertyGroup/TargetFrameworks")?.Value;
            if (targetFrameworks == null)
            {
                _logger.LogError("Could not determine target framework.");
                return -1;
            }

            if (!context.NoBuild)
            {
                var buildResult =
                    await BuildProjectAsync(projectFile, context.BuildConfiguration, true, cancellationToken);
                if (buildResult != 0) return buildResult;
            }

            var latestTargetFramework = targetFrameworks.Split(';').OrderByDescending(x => x).First();

            var projectName = projectFile.Name[..^projectFile.Extension.Length];

            var assemblyPath = Path.Combine(projectFile.DirectoryName!, "bin", context.BuildConfiguration,
                latestTargetFramework, $"{projectName}.dll");
            var loadContext = new DokiAssemblyLoadContext(assemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

            var documentationFile = new XPathDocument(Path.Combine(projectFile.DirectoryName!, "bin",
                context.BuildConfiguration, latestTargetFramework, $"{projectName}.xml"));

            context.Generator.AddAssembly(assembly, documentationFile, projectMetadata);

            _loadContexts.Add(loadContext);
        }

        return 0;
    }

    private async Task<int> LoadOutputsAsync(GenerateContext context, CancellationToken cancellationToken)
    {
        if (context.DokiConfig.Outputs == null)
        {
            _logger.LogError("No outputs configured.");
            return -1;
        }

        foreach (var output in context.DokiConfig.Outputs)
        {
            if (string.IsNullOrWhiteSpace(output.Type))
            {
                _logger.LogError("No output type configured.");
                return -1;
            }

            var outputType = await LoadOutputAsync(context.Directory, output, context.AllowPreview, cancellationToken);

            if (outputType == null)
            {
                _logger.LogError("Could not load output: {OutputType}", output.Type);
                return -1;
            }

            _logger.LogDebug("Adding output: {OutputType}", outputType);

            var outputAttribute = outputType.GetCustomAttribute<DokiOutputAttribute>();

            var methodName = outputAttribute?.Scoped == true
                ? nameof(DocumentationGenerator.AddScopedOutput)
                : nameof(DocumentationGenerator.AddOutput);

            var genericMethod = typeof(DocumentationGenerator).GetMethod(methodName)!.MakeGenericMethod(outputType);

            genericMethod.Invoke(context.Generator, []);
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

        _logger.LogDebug("dotnet {Arguments}", arguments);

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

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Build failed.");
            _logger.LogError(output);
            return -1;
        }

        _logger.LogDebug(output);

        _logger.LogInformation("[bold green]Build succeeded.[/]");

        //TODO add project dependencies
        _builtProjects.Add(projectFile.FullName);

        return 0;
    }

    private class GenerateContext
    {
        public DocumentationGenerator Generator { get; init; } = null!;

        public DokiConfig DokiConfig { get; init; } = null!;

        public string BuildConfiguration { get; init; } = null!;

        public bool NoBuild { get; init; }

        public bool AllowPreview { get; init; }

        public DirectoryInfo Directory { get; init; } = null!;
    }
}