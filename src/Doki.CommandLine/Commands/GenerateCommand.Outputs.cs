using System.Reflection;
using Doki.CommandLine.NuGet;
using Doki.Output.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal partial class GenerateCommand
{
    private async Task<OutputRegistration?> LoadOutputRegistrationAsync(FileSystemInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, bool allowPreview, CancellationToken cancellationToken)
    {
        if (output.From == null)
            return await LoadOutputRegistrationFromNuGetAsync(workingDirectory, output, allowPreview,
                cancellationToken);

        var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From));

        if (!fileInfo.Exists) return null;

        return fileInfo.Extension.ToLower() switch
        {
            ".csproj" => await LoadOutputRegistrationFromProjectAsync(fileInfo, cancellationToken),
            ".dll" => LoadOutputRegistrationFromAssembly(fileInfo.FullName),
            _ => null
        };
    }

    private async Task<OutputRegistration?> LoadOutputRegistrationFromNuGetAsync(FileSystemInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, bool allowPreview, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output registration from NuGet: {PackageId}", output.Type);

        var nugetFolder = Path.Combine(workingDirectory.FullName, ".doki", "nuget");

        using var nugetLoader = new NuGetLoader(_logger, output.From);

        var assemblyPath =
            await nugetLoader.LoadPackageAsync(output.Type, nugetFolder, allowPreview, cancellationToken);

        return LoadOutputRegistrationFromAssembly(assemblyPath);
    }

    private async Task<OutputRegistration?> LoadOutputRegistrationFromProjectAsync(FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output registration from project: {ProjectFileName}", fileInfo.Name);

        var fileName = fileInfo.Name[..^fileInfo.Extension.Length];

#if DEBUG
        const bool buildForDoki = true;
        const string buildConfiguration = "Debug";
#else
        const bool buildForDoki = false;
        const string buildConfiguration = "Release";
#endif

        var buildResult =
            await BuildProjectAsync(fileInfo, buildConfiguration, buildForDoki, cancellationToken);
        if (buildResult != 0) return null;

        var assemblyPath =
            Path.Combine(fileInfo.DirectoryName!, "bin", buildConfiguration, "net8.0", $"{fileName}.dll");

        return LoadOutputRegistrationFromAssembly(assemblyPath);
    }

    private OutputRegistration? LoadOutputRegistrationFromAssembly(string path)
    {
        _logger.LogDebug("Loading output registration from assembly: {AssemblyPath}", path);

        var assembly = Assembly.LoadFrom(path);

        var registrationMethod = assembly.GetExportedTypes()
            .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .FirstOrDefault(x => x.GetCustomAttribute<DokiOutputRegistrationAttribute>() != null);

        if (registrationMethod == null) return null;

        var parameters = registrationMethod.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IServiceCollection))
            return services =>
            {
                _logger.LogDebug("Using output registration method: {Method}", registrationMethod.Name);

                registrationMethod.Invoke(null, [services]);
            };

        _logger.LogError("Output registration method must have a single parameter of type IServiceCollection.");
        return null;
    }

    private delegate void OutputRegistration(IServiceCollection services);
}