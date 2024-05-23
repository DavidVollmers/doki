[Packages](../../README.md) / [Doki](../README.md) / [Doki](README.md) / 

# DocumentationGenerator Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.dll](../README.md)

Package: [Doki](https://www.nuget.org/packages/Doki)

---

**Generates documentation for assemblies.**

```csharp
public sealed class DocumentationGenerator
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → DocumentationGenerator

## Examples

 The following example shows how to generate documentation for an assembly and output it using the [MarkdownOutput](../../Doki.Output.Markdown/Doki.Output.Markdown/Doki.Output.Markdown.MarkdownOutput.md) class.
```csharp
var outputContext = new OutputContext(Directory.GetCurrentDirectory());

var generator = new DocumentationGenerator();

generator.AddAssembly(typeof(Program).Assembly, new XPathDocument("path/to/assembly.xml"));

generator.AddOutput(new MarkdownOutput(outputContext));

await generator.GenerateAsync(new ConsoleLogger());
```


## Remarks

 You can also use the doki cli tool to generate documentation. See the official [documentation](https://github.com/DavidVollmers/doki) for more information.

## Constructors

|   |Summary|
|---|---|
|DocumentationGenerator()| Initializes a new instance of the [DocumentationGenerator](Doki.DocumentationGenerator.md) class.|
|DocumentationGenerator(System.IServiceProvider)||
|DocumentationGenerator(System.Reflection.Assembly, System.Xml.XPath.XPathDocument)| Initializes a new instance of the [DocumentationGenerator](Doki.DocumentationGenerator.md) class with the specified assembly and xml documentation.|


## Properties

|   |Summary|
|---|---|
|TypeFilter| Gets the filter for types to include in the documentation.|
|ConstructorFilter| Gets the filter for constructors to include in the documentation.|
|FieldFilter| Gets the filter for fields to include in the documentation.|
|PropertyFilter| Gets the filter for properties to include in the documentation.|
|MethodFilter| Gets the filter for methods to include in the documentation.|
|IncludeInheritedMembers| Gets or sets a value indicating whether to include inherited members in the documentation.|


## Methods

|   |Summary|
|---|---|
|AddAssembly(System.Reflection.Assembly, System.Xml.XPath.XPathDocument, System.Xml.XPath.XPathDocument)| Adds an assembly to generate documentation for.|
|AddOutput(Doki.Output.IOutput)| Adds an output to write the documentation to.|
|GenerateAsync(Microsoft.Extensions.Logging.ILogger, System.Threading.CancellationToken)| Generates the documentation for the assemblies and writes it to the outputs.|


