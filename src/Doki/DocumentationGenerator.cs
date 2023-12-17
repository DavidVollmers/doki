using System.Reflection;
using System.Xml.XPath;
using Doki.Output.Abstractions;

namespace Doki;

public sealed class DocumentationGenerator
{
    private readonly Assembly _assembly;
    private readonly XPathDocument _documentation;
    private readonly List<IOutput> _outputs = [];

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
        
    }
}