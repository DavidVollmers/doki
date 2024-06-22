using System.Xml.XPath;
using Doki.TestAssembly.InheritanceChain;
using Doki.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace Doki.Extensions.Tests;

public class DocumentationRootExtensionTests
{
    [Fact]
    public async Task Test_TryGetParent_ParentPropertyIsNotNull()
    {
        var testOutput = new DocumentationRootCapture();

        var documentationGenerator = new DocumentationGenerator();

        var emptyDocumentation = new XPathDocument(new StringReader("""<?xml version="1.0"?><doc></doc>"""));

        documentationGenerator.AddOutput(testOutput);

        documentationGenerator.AddAssembly(typeof(SimpleClass).Assembly, emptyDocumentation);

        await documentationGenerator.GenerateAsync(NullLogger.Instance);

        Assert.NotNull(testOutput.Root);

        var assemblyDocumentation = testOutput.Root.Assemblies.Single();

        var namespaceDocumentation = assemblyDocumentation.Namespaces.Single();

        var simpleClassDocumentation = namespaceDocumentation.Types.First();

        Assert.NotNull(simpleClassDocumentation.Parent);

        var parent = testOutput.Root.TryGetParent(simpleClassDocumentation);

        Assert.NotNull(parent);
        Assert.Equal(namespaceDocumentation, parent);
    }
}