[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# MemberDocumentation Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents the documentation for a member.**

```csharp
public record MemberDocumentation : Doki.DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → MemberDocumentation

Derived: [TypeDocumentationReference](Doki.TypeDocumentationReference.md)

Implements: [System.IEquatable&lt;Doki.MemberDocumentation&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.MemberDocumentation&gt;)

## Constructors

|   |Summary|
|---|---|
|MemberDocumentation(Doki.MemberDocumentation)||
|MemberDocumentation()||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Name| Gets the name of the member.|
|Namespace| Gets the namespace of the member.|
|Assembly| Gets the assembly of the member.|
|Summaries| Gets the summary of the member.|
|Examples| Get the examples of the member.|
|Remarks| Gets the remarks of the member.|
|IsDocumented| Gets a value indicating whether the type is documented.|
|ContentType||


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.MemberDocumentation)||


