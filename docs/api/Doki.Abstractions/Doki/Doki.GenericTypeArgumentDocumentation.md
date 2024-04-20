[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# GenericTypeArgumentDocumentation Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a generic type argument in the documentation.**

```csharp
public sealed record GenericTypeArgumentDocumentation : Doki.TypeDocumentationReference
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → [MemberDocumentation](Doki.MemberDocumentation.md) → [TypeDocumentationReference](Doki.TypeDocumentationReference.md) → GenericTypeArgumentDocumentation

Implements: [System.IEquatable&lt;Doki.GenericTypeArgumentDocumentation&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.GenericTypeArgumentDocumentation&gt;)

## Constructors

|   |Summary|
|---|---|
|GenericTypeArgumentDocumentation()||


## Properties

|   |Summary|
|---|---|
|Description|Gets the description of the generic type argument.|
|IsGenericParameter|Gets a value indicating whether the generic type argument is a generic parameter.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.TypeDocumentationReference)||
|Equals(Doki.GenericTypeArgumentDocumentation)||


