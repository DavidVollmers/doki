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
        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions.dll", documentation.FileName);
        Assert.Equal("1.0.0.0", documentation.Version);
        Assert.Null(documentation.PackageId);
        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", documentation.Name);
        Assert.Null(documentation.Description);
        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", documentation.Id);
        Assert.Equal(DocumentationContentType.Assembly, documentation.ContentType);
        Assert.Collection(documentation.Namespaces,
            namespaceDocumentation =>
            {
                Assert.NotNull(namespaceDocumentation);
                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", namespaceDocumentation.Name);
                Assert.Null(namespaceDocumentation.Description);
                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", namespaceDocumentation.Id);
                Assert.Equal(DocumentationContentType.Namespace, namespaceDocumentation.ContentType);
                Assert.Collection(namespaceDocumentation.Types,
                    typeDocumentation =>
                    {
                        Assert.NotNull(typeDocumentation);
                        Assert.Equal("public abstract class AbstractClass", typeDocumentation.Definition);
                        Assert.Equal(DocumentationContentType.Class, typeDocumentation.ContentType);
                        Assert.False(typeDocumentation.IsGeneric);
                        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass",
                            typeDocumentation.FullName);
                        Assert.True(typeDocumentation.IsDocumented);
                        Assert.False(typeDocumentation.IsMicrosoft);
                        Assert.Equal("AbstractClass", typeDocumentation.Name);
                        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", typeDocumentation.Namespace);
                        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions", typeDocumentation.Assembly);
                        Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass",
                            typeDocumentation.Id);
                        Assert.Empty(typeDocumentation.Remarks);
                        Assert.Empty(typeDocumentation.Interfaces);
                        Assert.Empty(typeDocumentation.DerivedTypes);
                        Assert.Empty(typeDocumentation.Fields);
                        Assert.Empty(typeDocumentation.Properties);
                        Assert.Empty(typeDocumentation.Methods);
                        Assert.Empty(typeDocumentation.GenericArguments);

                        Assert.NotNull(typeDocumentation.BaseType);
                        Assert.False(typeDocumentation.BaseType.IsGeneric);
                        Assert.Equal("System.Object", typeDocumentation.BaseType.FullName);
                        Assert.False(typeDocumentation.BaseType.IsDocumented);
                        Assert.True(typeDocumentation.BaseType.IsMicrosoft);
                        Assert.Null(typeDocumentation.BaseType.BaseType);
                        Assert.Empty(typeDocumentation.BaseType.GenericArguments);
                        Assert.Equal("Object", typeDocumentation.BaseType.Name);
                        Assert.Equal("System", typeDocumentation.BaseType.Namespace);
                        Assert.Equal("System.Private.CoreLib", typeDocumentation.BaseType.Assembly);
                        Assert.Null(typeDocumentation.BaseType.Summary);
                        Assert.Equal(DocumentationContentType.TypeReference, typeDocumentation.BaseType.ContentType);
                        Assert.Equal("System.Object", typeDocumentation.BaseType.Id);

                        Assert.Collection(typeDocumentation.Constructors,
                            memberDocumentation =>
                            {
                                Assert.NotNull(memberDocumentation);
                                Assert.Equal("AbstractClass()", memberDocumentation.Name);
                                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions",
                                    memberDocumentation.Namespace);
                                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions",
                                    memberDocumentation.Assembly);
                                Assert.Null(memberDocumentation.Summary);
                                Assert.Equal(DocumentationContentType.Constructor, memberDocumentation.ContentType);
                                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass.#ctor",
                                    memberDocumentation.Id);
                            });

                        Assert.NotNull(typeDocumentation.Summary);
                        Assert.Equal("summary", typeDocumentation.Summary.Name);
                        Assert.Equal("summary", typeDocumentation.Summary.Id);
                        Assert.Equal(DocumentationContentType.Xml, typeDocumentation.Summary.ContentType);
                        Assert.Collection(typeDocumentation.Summary.Contents,
                            documentationObject =>
                            {
                                Assert.NotNull(documentationObject);
                                Assert.IsType<TextContent>(documentationObject);
                                var textContent = (TextContent)documentationObject;
                                Assert.Equal("This is an abstract class. See ", textContent.Text);
                                Assert.Equal(DocumentationContentType.Text, documentationObject.ContentType);
                                Assert.Equal("text", documentationObject.Id);
                            },
                            documentationObject =>
                            {
                                Assert.NotNull(documentationObject);
                                Assert.IsType<TypeDocumentationReference>(documentationObject);
                                var typeDocumentationReference = (TypeDocumentationReference)documentationObject;
                                Assert.False(typeDocumentationReference.IsGeneric);
                                Assert.Equal("Doki.TestAssembly.InheritanceChain.Abstractions.AbstractClass",
                                    typeDocumentationReference.FullName);
                                Assert.True(typeDocumentationReference.IsDocumented);
                                Assert.False(typeDocumentationReference.IsMicrosoft);
                            },
                            documentationObject =>
                            {
                                Assert.NotNull(documentationObject);
                                Assert.IsType<TextContent>(documentationObject);
                                var textContent = (TextContent)documentationObject;
                                Assert.Equal("for more information.", textContent.Text.Trim());
                                Assert.Equal(DocumentationContentType.Text, documentationObject.ContentType);
                                Assert.Equal("text", documentationObject.Id);
                            });
                    });
            });
    }
}