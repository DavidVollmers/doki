[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# TypeDocumentationReference Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a type documentation reference in the documentation.**

```csharp
public record TypeDocumentationReference : Doki.MemberDocumentation
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → [MemberDocumentation](Doki.MemberDocumentation.md) → TypeDocumentationReference

Derived: [GenericTypeArgumentDocumentation](Doki.GenericTypeArgumentDocumentation.md), [TypeDocumentation](Doki.TypeDocumentation.md)

Implements: [System.IEquatable&lt;Doki.TypeDocumentationReference&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.TypeDocumentationReference&gt;)

## Constructors

|   |Summary|
|---|---|
|TypeDocumentationReference()| Initializes a new instance of the [TypeDocumentationReference](Doki.TypeDocumentationReference.md) class.|
|TypeDocumentationReference(Doki.TypeDocumentationReference)||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|IsGeneric| Gets a value indicating whether the type is generic.|
|FullName| Gets the full name of the type.|
|IsMicrosoft| Gets a value indicating whether the type is from Microsoft.|
|BaseType| Gets the base type of the type.|
|GenericArguments| Gets the generic arguments of the type.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.MemberDocumentation)||
|Equals(Doki.TypeDocumentationReference)||


