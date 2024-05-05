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
|MemberDocumentation()|Initializes a new instance of the [MemberDocumentation](Doki.MemberDocumentation.md) class.|
|MemberDocumentation(Doki.MemberDocumentation)|Initializes a new instance of the [MemberDocumentation](Doki.MemberDocumentation.md) class.|


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Name|Gets the name of the member.|
|Namespace|Gets the namespace of the member.|
|Assembly|Gets the assembly of the member.|
|Summary|Gets the summary of the member.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.MemberDocumentation)||

