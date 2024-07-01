[Packages](../../../README.md) / [Doki.Abstractions](../../README.md) / [Doki](../README.md) / 

# AssemblyDocumentation Class

## Definition

Namespace: [Doki](../README.md)

Assembly: [Doki.Abstractions.dll](../../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents the documentation for an assembly.**

```csharp
public sealed record AssemblyDocumentation : Doki.DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](../Doki.DocumentationObject/README.md) → AssemblyDocumentation

Implements: [System.IEquatable&lt;Doki.AssemblyDocumentation&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.AssemblyDocumentation&gt;)

## Constructors

|   |Summary|
|---|---|
|AssemblyDocumentation()||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|FileName| Gets the name of the assembly.|
|Version| Gets the version of the assembly.|
|PackageId| Gets the NuGet package ID of the assembly.|
|Name||
|Description||
|Namespaces||


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.AssemblyDocumentation)||


