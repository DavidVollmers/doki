using System.Xml.XPath;
using Doki.Output;
using Doki.Output.Markdown;
using Doki.TestAssembly;
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

        generator.AddOutput(new MarkdownOutput(snapshot.Context));

        await generator.GenerateAsync(NullLogger.Instance);

        await snapshot.SaveIfNotExists().MatchSnapshotAsync(testOutputHelper);
    }
}