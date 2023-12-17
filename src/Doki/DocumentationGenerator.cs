using System.Reflection;
using System.Xml.XPath;
using Doki.Elements;
using Doki.Output;

namespace Doki;

public sealed class DocumentationGenerator
{
    private readonly Assembly _assembly;
    private readonly XPathDocument _documentation;
    private readonly List<IOutput> _outputs = [];

    private XPathNavigator? _navigator;
    private XPathNavigator Navigator => _navigator ??= _documentation.CreateNavigator();

    public DocumentationGenerator(Assembly assembly, XPathDocument documentation)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(documentation);

        _assembly = assembly;
        _documentation = documentation;
    }

    public void AddOutput(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (_outputs.Contains(output)) throw new ArgumentException("Output already added.", nameof(output));

        _outputs.Add(output);
    }

    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        var exportedTypes = _assembly.GetExportedTypes();

        var namespaces = exportedTypes.Select(t => t.Namespace).Distinct().ToList();

        var namespacesTableOfContents = new TableOfContents
        {
            Id = Guid.NewGuid(),
            Name = "Namespaces",
            Children = namespaces.Select(n => new TableOfContents
            {
                Id = Guid.NewGuid(),
                Name = n!,
                Children = exportedTypes.Where(t => t.Namespace == n).Select(t => new TableOfContents
                {
                    Id = t.GUID,
                    Name = t.Name,
                }).ToArray()
            }).ToArray()
        };

        foreach (var output in _outputs)
        {
            await output.WriteAsync(namespacesTableOfContents, cancellationToken);
        }

        //TODO generate ToCs for types in namespaces

        foreach (var exportedType in exportedTypes)
        {
            await GenerateTypeDocumentationAsync(exportedType, cancellationToken);
        }
    }

    private async Task GenerateTypeDocumentationAsync(Type type, CancellationToken cancellationToken = default)
    {
        var typeDocumentation = Navigator.SelectSingleNode($"//doc//members//member[@name='T:{type}]");
    }
}