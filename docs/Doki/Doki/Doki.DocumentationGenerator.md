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

---

## Examples

The following example shows how to generate documentation for an assembly and output it using the  class.

```csharp
var outputContext = new OutputContext(Directory.GetCurrentDirectory());

var generator = new DocumentationGenerator();

generator.AddAssembly(typeof(Program).Assembly, new XPathDocument("path/to/assembly.xml"));

generator.AddOutput(new MarkdownOutput(outputContext));

await generator.GenerateAsync(new ConsoleLogger());

```

