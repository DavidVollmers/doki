using System.Reflection;
using Doki.CommandLine.NuGet;
using Doki.Output;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Doki.CommandLine.Commands;

internal partial class GenerateCommand
{
    private readonly SourceRepository _nuget = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
    private readonly SourceCacheContext _cacheContext = new();

    private async Task<IOutput?> LoadOutputAsync(DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, bool allowPreview, CancellationToken cancellationToken)
    {
        var outputContext = new OutputContext(workingDirectory, output.Options);

        if (output.From == null)
            return await LoadOutputFromNuGetAsync(output, outputContext, allowPreview, cancellationToken);

        var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From));

        if (!fileInfo.Exists) return null;

        return fileInfo.Extension.ToLower() switch
        {
            ".csproj" => await LoadOutputFromProjectAsync(fileInfo, output, outputContext, cancellationToken),
            ".dll" => LoadOutputFromAssembly(fileInfo.FullName, output, outputContext),
            _ => null
        };
    }

    private async Task<IOutput?> LoadOutputFromNuGetAsync(DokiConfig.DokiConfigOutput output,
        OutputContext outputContext, bool allowPreview, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output from NuGet: {PackageId}", output.Type);

        var nugetFolder = Path.Combine(outputContext.WorkingDirectory.FullName, ".doki", "nuget");

        using var nugetLoader = new NuGetLoader(output.From);

        var assemblyPath =
            await nugetLoader.LoadPackageAsync(output.Type, nugetFolder, allowPreview, cancellationToken);

        return LoadOutputFromAssembly(assemblyPath, output, outputContext);
    }

    private async Task<IOutput?> LoadOutputFromProjectAsync(FileInfo fileInfo, DokiConfig.DokiConfigOutput output,
        OutputContext outputContext, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output from project: {ProjectFileName}", fileInfo.Name);

        var buildResult = await BuildProjectAsync(fileInfo, "Release", false, cancellationToken);
        if (buildResult != 0) return null;

        var assemblyPath = Path.Combine(fileInfo.DirectoryName!, "bin", "Release", "net8.0",
            $"{fileInfo.Name[..^fileInfo.Extension.Length]}.dll");

        return LoadOutputFromAssembly(assemblyPath, output, outputContext);
    }

    private IOutput? LoadOutputFromAssembly(string path, DokiConfig.DokiConfigOutput output,
        OutputContext outputContext)
    {
        _logger.LogDebug("Loading output from assembly: {AssemblyPath}", path);

        var assembly = Assembly.LoadFrom(path);

        var outputType = assembly.GetType(output.Type) ?? assembly
            .GetExportedTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<DokiOutputAttribute>()?.Name == output.Type);

        // ReSharper disable once InvertIf
        if (outputType == null)
        {
            _logger.LogError("Could not find output type: {OutputType}", output.Type);
            return null;
        }

        return Activator.CreateInstance(outputType, outputContext) as IOutput;
    }
}