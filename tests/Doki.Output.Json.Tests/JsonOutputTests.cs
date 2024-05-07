using System.Text.Json;
using System.Xml.XPath;
using Doki.TestAssembly.InheritanceChain.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Doki.Output.Json.Tests;

public class JsonOutputTests
{
    [Fact]
    public async Task Test_Deserialization()
    {
        var outputDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "doki"));

        var emptyDocumentation = new XPathDocument(new StringReader("""
                                                                    <?xml version="1.0"?>
                                                                    <doc>
                                                                        <assembly>
                                                                            <name>Doki.TestAssembly.InheritanceChain.Abstractions</name>
                                                                        </assembly>
                                                                        <members>
                                                                            <member name="T:Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass">
                                                                                <summary>
                                                                                This is an abstract class. See <see cref="T:Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass"/> for more information.
                                                                                </summary>
                                                                                <example>
                                                                                This is an example of how to use the <see cref="T:Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass"/> class.
                                                                                <code>
                                                                                public class ExampleClass : AbstractClass {}
                                                                                </code>
                                                                                </example>
                                                                            </member>
                                                                        </members>
                                                                    </doc>
                                                                    """));

        var generator = new DocumentationGenerator();

        generator.AddAssembly(typeof(AbstractClass).Assembly, emptyDocumentation);

        generator.AddOutput(new JsonOutput(new OutputOptions<JsonOutput>
        {
            OutputDirectory = outputDirectory
        }));

        await generator.GenerateAsync(NullLogger.Instance);

        var jsonFile = outputDirectory.GetFiles().Single();

        var json = await File.ReadAllTextAsync(jsonFile.FullName);

        var documentation = JsonSerializer.Deserialize<AssemblyDocumentation>(json);
        Assert.NotNull(documentation);
        
        
    }
}