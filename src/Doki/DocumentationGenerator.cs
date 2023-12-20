using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.XPath;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

public sealed class DocumentationGenerator
{
    private readonly Dictionary<Assembly, XPathNavigator> _assemblies = new();
    private readonly Dictionary<string, XPathNavigator> _projectMetadata = new();
    private readonly List<IOutput> _outputs = [];

    public DocumentationGenerator()
    {
    }

    public DocumentationGenerator(Assembly assembly, XPathDocument documentation)
    {
        AddAssembly(assembly, documentation);
    }

    public void AddAssembly(Assembly assembly, XPathDocument documentation, XPathDocument? projectMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(documentation);

        _assemblies.Add(assembly, documentation.CreateNavigator());

        if (projectMetadata != null)
        {
            _projectMetadata.Add(assembly.GetName().Name!, projectMetadata.CreateNavigator());
        }
    }

    public void AddOutput(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _outputs.Add(output);
    }

    public async Task GenerateAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logger);

        if (_assemblies.Count == 0) throw new InvalidOperationException("No assemblies added for documentation.");

        logger.LogInformation("Generating documentation for {AssemblyCount} assemblies.", _assemblies.Count);

        var assembliesToC = new TableOfContents
        {
            Id = TableOfContents.Assemblies,
            Content = DokiContent.Assemblies
        };

        var children = new List<TableOfContents>();
        foreach (var (assembly, _) in _assemblies)
        {
            string? description = null;
            var assemblyId = assembly.GetName().Name!;
            if (_projectMetadata.TryGetValue(assemblyId, out var projectMetadata))
            {
                var packageId = projectMetadata.SelectSingleNode("/Project/PropertyGroup/PackageId")?.Value;
                if (packageId != null) assemblyId = packageId;

                description = projectMetadata.SelectSingleNode("/Project/PropertyGroup/Description")?.Value;
            }

            var assemblyToC = new TableOfContents
            {
                Id = assemblyId,
                Parent = assembliesToC,
                Content = DokiContent.Assembly,
                Properties = new Dictionary<string, object?>
                {
                    {"Description", description}
                }
            };

            var namespaceToCs = GenerateAssemblyDocumentationAsync(assemblyToC, assembly, logger, cancellationToken);

            assemblyToC.Children = await namespaceToCs.ToArrayAsync(cancellationToken);

            children.Add(assemblyToC);
        }

        assembliesToC.Children = children.ToArray();

        foreach (var output in _outputs)
        {
            await output.WriteAsync(assembliesToC, cancellationToken);
        }
    }

    private async IAsyncEnumerable<TableOfContents> GenerateAssemblyDocumentationAsync(TableOfContents parent,
        Assembly assembly, ILogger logger, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var types = GetTypesToDocument(assembly).ToArray();

        logger.LogInformation("Generating documentation for {TypeCount} types.", types.Length);

        var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

        foreach (var @namespace in namespaces)
        {
            var namespaceToC = new TableOfContents
            {
                Id = @namespace!,
                Parent = parent,
                Content = DokiContent.Namespace
            };

            var children = new List<TableOfContents>();
            foreach (var type in types.Where(t => t.Namespace == @namespace))
            {
                var typeDocumentation = await GenerateTypeDocumentationAsync(type, logger, cancellationToken);

                children.Add(new TableOfContents
                {
                    Id = type.FullName!,
                    Parent = namespaceToC,
                    Content = DokiContent.TypeReference,
                    Properties = typeDocumentation.Properties
                });
            }

            namespaceToC.Children = children.ToArray();

            yield return namespaceToC;
        }
    }

    private async Task<TypeDocumentation> GenerateTypeDocumentationAsync(Type type, ILogger logger,
        CancellationToken cancellationToken)
    {
        var navigator = _assemblies[type.Assembly];

        var typeXml = navigator.SelectSingleNode($"//doc//members//member[@name='T:{type}']");

        return new TypeDocumentation();
    }

    //TODO support exclude filtering
    private static IEnumerable<Type> GetTypesToDocument(Assembly assembly)
    {
        return assembly.GetTypes().Where(a => a.IsPublic);
    }
}