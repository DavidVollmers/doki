[Packages](../../README.md) / [Doki.Abstractions](../README.md) / [Doki](README.md) / 

# Link Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.Abstractions.dll](../README.md)

Package: [Doki.Abstractions](https://www.nuget.org/packages/Doki.Abstractions)

---

**Represents a link in the documentation.**

```csharp
public sealed record Link : Doki.DocumentationObject
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → [DocumentationObject](Doki.DocumentationObject.md) → Link

Implements: [System.IEquatable&lt;Doki.Link&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.Link&gt;)

## Constructors

|   |Summary|
|---|---|
|Link()||


## Properties

|   |Summary|
|---|---|
|EqualityContract||
|Url|Gets the URL of the link.|
|Text|Gets the text of the link.|


## Methods

|   |Summary|
|---|---|
|ToString()||
|PrintMembers(System.Text.StringBuilder)||
|GetHashCode()||
|Equals(System.Object)||
|Equals(Doki.DocumentationObject)||
|Equals(Doki.Link)||


