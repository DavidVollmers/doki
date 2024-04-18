using System.Reflection;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal partial class BuildCommand
{
    private async Task<IOutput?> LoadOutputAsync(DirectoryInfo workingDirectory,
        DokiConfig.DokiConfigOutput output, CancellationToken cancellationToken)
    {
        if (output.From == null) return null;

        var outputContext = new OutputContext(workingDirectory, output.Options);

        var fileInfo = new FileInfo(Path.Combine(workingDirectory.FullName, output.From!));

        //TODO load nuget package
        if (!fileInfo.Exists) return null;

        return fileInfo.Extension.ToLower() switch
        {
            ".csproj" => await LoadOutputFromProjectAsync(fileInfo, output, outputContext, cancellationToken),
            ".dll" => LoadOutputFromAssembly(fileInfo.FullName, output, outputContext),
            _ => null
        };
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

        var outputType = assembly.GetType(output.Type!) ?? assembly
            .GetExportedTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<DokiOutputAttribute>()?.Name == output.Type);

        if (outputType == null) return null;

        return Activator.CreateInstance(outputType, outputContext) as IOutput;
    }
}