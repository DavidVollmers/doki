using System.Reflection;
using Doki.Output;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Doki.CommandLine.Commands;

internal partial class BuildCommand
{
    private readonly SourceRepository _nuget = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
    private readonly SourceCacheContext _cacheContext = new();

    private async Task<IOutput?> LoadOutputAsync(DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, CancellationToken cancellationToken)
    {
        if (output.From == null) return null;

        var outputContext = new OutputContext(workingDirectory, output.Options);

        var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From));

        //TODO load nuget package
        if (!fileInfo.Exists) return await LoadOutputFromNuGetAsync(output, outputContext, cancellationToken);

        return fileInfo.Extension.ToLower() switch
        {
            ".csproj" => await LoadOutputFromProjectAsync(fileInfo, output, outputContext, cancellationToken),
            ".dll" => LoadOutputFromAssembly(fileInfo.FullName, output, outputContext),
            _ => null
        };
    }

    //TODO support custom registries (using output.From)
    private async Task<IOutput?> LoadOutputFromNuGetAsync(DokiConfig.DokiConfigOutput output,
        OutputContext outputContext, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output from NuGet: {PackageId}", output.Type);

        var packages = await _nuget.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

        var versions = await packages.GetAllVersionsAsync(output.Type, _cacheContext, NullLogger.Instance,
            cancellationToken);

        var latestVersion = versions.Where(v => v.OriginalVersion?.EndsWith("-preview") != true)
            .OrderByDescending(v => v).First();

        var package = new PackageIdentity(output.From, latestVersion);

        var downloader =
            await packages.GetPackageDownloaderAsync(package, _cacheContext, NullLogger.Instance, cancellationToken);

        var destination = new FileInfo(Path.Combine(outputContext.WorkingDirectory.FullName, ".doki", "cache",
            "output", $"{output.Type}.nupkg"));

        var result = await downloader.CopyNupkgFileToAsync(destination.FullName, cancellationToken);

        if (!result) return null;
        
        //TODO extract package

        return null;
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

        if (outputType == null) return null;

        return Activator.CreateInstance(outputType, outputContext) as IOutput;
    }
}