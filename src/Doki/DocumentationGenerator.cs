using System.Reflection;
using System.Xml.XPath;
using Doki.Elements;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

public sealed class DocumentationGenerator
{
    private readonly IDictionary<Assembly, XPathDocument> _assemblies = new Dictionary<Assembly, XPathDocument>();
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

        _assemblies.Add(assembly, documentation);
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

        logger.LogInformation("Generating namespace documentation...");

        await GenerateNamespaceDocumentationAsync(types, cancellationToken);

        logger.LogInformation("Generating type documentation...");

        foreach (var exportedType in types)
        {
            await GenerateTypeDocumentationAsync(exportedType, cancellationToken);
        }
    }

    private async Task GenerateTypeDocumentationAsync(Type type, CancellationToken cancellationToken)
    {
        // var typeDocumentation = Navigator.SelectSingleNode($"//doc//members//member[@name='T:{type}']");
    }

    private async Task GenerateNamespaceDocumentationAsync(Type[] types, CancellationToken cancellationToken)
    {
        var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

        var namespacesTableOfContents = new TableOfContents
        {
            Id = Guid.NewGuid(),
            Name = "Namespaces",
        };

        namespacesTableOfContents.Children = namespaces.Select(n =>
        {
            var subToC = new TableOfContents
            {
                Id = Guid.NewGuid(),
                Name = n!,
                Parent = namespacesTableOfContents
            };

            subToC.Children = types.Where(t => t.Namespace == n).Select(t => new TableOfContents
            {
                Id = t.GUID,
                Name = t.Name,
                Parent = subToC
            }).ToArray();

            return subToC;
        }).ToArray();

        foreach (var output in _outputs)
        {
            await output.WriteAsync(namespacesTableOfContents, cancellationToken);
        }

        //TODO generate ToCs for types in namespaces
    }

    //TODO support exclude filtering
    private IEnumerable<Type> CollectTypesToDocument()
    {
        if (_assemblies.Count == 0) throw new InvalidOperationException("No assemblies added for documentation.");

        return _assemblies.SelectMany(assembly => assembly.Key.GetTypes().Where(a => a.IsPublic));
    }
}