using System.Reflection;
using System.Xml.XPath;
using Doki.Elements;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

public sealed class DocumentationGenerator
{
    private readonly Dictionary<Assembly, XPathNavigator> _assemblies = new();
    private readonly List<IOutput> _outputs = [];

    public DocumentationGenerator()
    {
    }

    public DocumentationGenerator(Assembly assembly, XPathDocument documentation)
    {
        AddAssembly(assembly, documentation);
    }

    public void AddAssembly(Assembly assembly, XPathDocument documentation)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(documentation);

        _assemblies.Add(assembly, documentation.CreateNavigator());
    }

    public void AddOutput(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _outputs.Add(output);
    }

    public async Task GenerateAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logger);

        var types = CollectTypesToDocument().ToArray();

        logger.LogInformation("Generating documentation for {TypeCount} types.", types.Length);

        await GenerateNamespaceDocumentationAsync(types, logger, cancellationToken);

        logger.LogInformation("Generating type documentation...");

        foreach (var exportedType in types)
        {
            await GenerateTypeDocumentationAsync(exportedType, cancellationToken);
        }
    }

    private async Task GenerateTypeDocumentationAsync(Type type, CancellationToken cancellationToken)
    {
        var navigator = _assemblies[type.Assembly];

        var typeDocumentation = navigator.SelectSingleNode($"//doc//members//member[@name='T:{type}']");
    }

    private async Task GenerateNamespaceDocumentationAsync(Type[] types, ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating namespace documentation...");

        var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

        var namespacesToC = new TableOfContents
        {
            Id = Guid.NewGuid(),
            Name = TableOfContents.Namespaces,
            Content = DokiContent.Namespaces
        };

        var children = new List<TableOfContents>();
        foreach (var @namespace in namespaces)
        {
            var namespaceToC = new TableOfContents
            {
                Id = Guid.NewGuid(),
                Name = @namespace!,
                Parent = namespacesToC,
                Content = DokiContent.Namespace
            };

            namespaceToC.Children = types.Where(t => t.Namespace == @namespace).Select(t => new TableOfContents
            {
                Id = t.GUID,
                Name = t.GetTypeInfo().GetSanitizedName(),
                Parent = namespaceToC,
                Content = DokiContent.TypeReference
            }).ToArray();

            children.Add(namespaceToC);
        }

        namespacesToC.Children = children.ToArray();

        foreach (var output in _outputs)
        {
            await output.WriteAsync(namespacesToC, cancellationToken);
        }
    }

    //TODO support exclude filtering
    private IEnumerable<Type> CollectTypesToDocument()
    {
        if (_assemblies.Count == 0) throw new InvalidOperationException("No assemblies added for documentation.");

        return _assemblies.SelectMany(assembly => assembly.Key.GetTypes().Where(a => a.IsPublic));
    }
}