[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# ContentList Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a list of content.**

```csharp
public record ContentList : Doki.DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → ContentList

Derived: [AssemblyDocumentation](Doki.AssemblyDocumentation.md)

Implements: [System.IEquatable&lt;Doki.ContentList&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.ContentList&gt;)

## Constructors

|   |Summary|
|---|---|
|ContentList(Doki.ContentList)||
|ContentList()||


## Fields

|   |Summary|
|---|---|
|Assemblies|Represents the assemblies content list.|


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Name|Gets the name of the content list.|
|Description|Gets the description of the content list.|
|Items|Gets the items in the content list.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.ContentList)||


