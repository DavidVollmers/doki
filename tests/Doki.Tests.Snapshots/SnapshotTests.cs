using System.Xml.XPath;
using Doki.Output;
using Doki.Output.Markdown;
using Doki.TestAssembly;
using Doki.TestAssembly.InheritanceChain;
using Doki.TestAssembly.InheritanceChain.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Doki.Tests.Snapshots;

public class SnapshotTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Test_RootNamespaceIsParentNamespace()
    {
        var snapshot = Snapshot.Create();

        var emptyDocumentation = new XPathDocument(new StringReader("""<?xml version="1.0"?><doc></doc>"""));

        var generator = new DocumentationGenerator();

        generator.AddAssembly(typeof(TestParentRootNamespaceClass).Assembly, emptyDocumentation);

        generator.AddOutput(new MarkdownOutput(new OutputOptions<MarkdownOutput>
        {
            OutputDirectory = snapshot.OutputDirectory
        }));

        await generator.GenerateAsync(NullLogger.Instance);

        await snapshot.SaveIfNotExists().MatchSnapshotAsync(testOutputHelper);
    }

    [Fact]
    public async Task Test_InheritanceChain()
    {
        var snapshot = Snapshot.Create();

        var emptyDocumentation = new XPathDocument(new StringReader("""<?xml version="1.0"?><doc></doc>"""));

        var generator = new DocumentationGenerator();

        generator.AddAssembly(typeof(AbstractClass).Assembly, emptyDocumentation);
        generator.AddAssembly(typeof(SimpleClass).Assembly, emptyDocumentation);

        generator.AddOutput(new MarkdownOutput(new OutputOptions<MarkdownOutput>
        {
            OutputDirectory = snapshot.OutputDirectory
        }));

        await generator.GenerateAsync(NullLogger.Instance);

        await snapshot.SaveIfNotExists().MatchSnapshotAsync(testOutputHelper);
    }
}