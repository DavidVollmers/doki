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
    private async Task<Type?> LoadOutputAsync(FileSystemInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, bool allowPreview, CancellationToken cancellationToken)
    {
        if (output.From == null)
            return await LoadOutputFromNuGetAsync(workingDirectory, output, allowPreview, cancellationToken);

        var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From));

        if (!fileInfo.Exists) return null;

        return fileInfo.Extension.ToLower() switch
        {
            ".csproj" => await LoadOutputFromProjectAsync(fileInfo, output, cancellationToken),
            ".dll" => LoadOutputFromAssembly(fileInfo.FullName, output),
            _ => null
        };
    }

    private async Task<Type?> LoadOutputFromNuGetAsync(FileSystemInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, bool allowPreview, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output from NuGet: {PackageId}", output.Type);

        var nugetFolder = Path.Combine(workingDirectory.FullName, ".doki", "nuget");

        using var nugetLoader = new NuGetLoader(output.From);

        var assemblyPath =
            await nugetLoader.LoadPackageAsync(output.Type, nugetFolder, allowPreview, cancellationToken);

        return LoadOutputFromAssembly(assemblyPath, output);
    }

    private async Task<Type?> LoadOutputFromProjectAsync(FileInfo fileInfo, DokiConfig.DokiConfigOutput output,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading output from project: {ProjectFileName}", fileInfo.Name);

        var buildResult = await BuildProjectAsync(fileInfo, "Release", false, cancellationToken);
        if (buildResult != 0) return null;

        var assemblyPath = Path.Combine(fileInfo.DirectoryName!, "bin", "Release", "net8.0",
            $"{fileInfo.Name[..^fileInfo.Extension.Length]}.dll");

        return LoadOutputFromAssembly(assemblyPath, output);
    }

    private Type? LoadOutputFromAssembly(string path, DokiConfig.DokiConfigOutput output)
    {
        _logger.LogDebug("Loading output from assembly: {AssemblyPath}", path);

        var assembly = Assembly.LoadFrom(path);

        return assembly.GetType(output.Type) ?? assembly
            .GetExportedTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<DokiOutputAttribute>()?.Name == output.Type);
    }
}