[Packages](../../../README.md) / [Doki.Abstractions](../../README.md) / [Doki](../README.md) / 

# TypeDocumentation Class

## Definition

Namespace: [Doki](../README.md)

Assembly: [Doki.Abstractions.dll](../../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a type documentation.**

```csharp
public sealed record TypeDocumentation : Doki.TypeDocumentationReference
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](../Doki.DocumentationObject/README.md) → [MemberDocumentation](../Doki.MemberDocumentation/README.md) → [TypeDocumentationReference](../Doki.TypeDocumentationReference/README.md) → TypeDocumentation

Implements: [System.IEquatable&lt;Doki.TypeDocumentation&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.TypeDocumentation&gt;)

## Constructors

|   |Summary|
|---|---|
|TypeDocumentation()||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Definition| Gets the definition of the type.|
|Interfaces| Gets the interfaces implemented by the type.|
|DerivedTypes| Gets the derived types of the type.|
|Constructors| Gets the constructors of the type.|
|Fields| Gets the fields of the type.|
|Properties| Gets the properties of the type.|
|Methods| Gets the methods of the type.|
|ContentType||


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.TypeDocumentationReference)||
|Equals(Doki.TypeDocumentation)||


