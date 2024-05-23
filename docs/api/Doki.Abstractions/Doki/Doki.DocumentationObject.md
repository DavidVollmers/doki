[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# DocumentationObject Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a documentation object.**

```csharp
public abstract record DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → DocumentationObject

Derived: [AssemblyDocumentation](Doki.AssemblyDocumentation.md), [CodeBlock](Doki.CodeBlock.md), [DocumentationRoot](Doki.DocumentationRoot.md), [Link](Doki.Link.md), [MemberDocumentation](Doki.MemberDocumentation.md), [NamespaceDocumentation](Doki.NamespaceDocumentation.md), [TextContent](Doki.TextContent.md), [XmlDocumentation](Doki.XmlDocumentation.md)

Implements: [System.IEquatable&lt;Doki.DocumentationObject&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.DocumentationObject&gt;)

## Constructors

|   |Summary|
|---|---|
|DocumentationObject()||
|DocumentationObject(Doki.DocumentationObject)||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Id| Gets the ID of the documentation object.|
|Parent| Gets the parent of the documentation object.|
|ContentType| Gets the content type of the documentation object.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||


