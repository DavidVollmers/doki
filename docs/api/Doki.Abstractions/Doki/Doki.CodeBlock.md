[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# CodeBlock Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a code block in the documentation.**

```csharp
public sealed record CodeBlock : Doki.DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → CodeBlock

Implements: [System.IEquatable&lt;Doki.CodeBlock&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.CodeBlock&gt;)

## Constructors

|   |Summary|
|---|---|
|CodeBlock()||


## Properties

|   |Summary|
|---|---|
|Language|Gets the language of the code block.|
|Code|Gets the code of the block.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.CodeBlock)||


