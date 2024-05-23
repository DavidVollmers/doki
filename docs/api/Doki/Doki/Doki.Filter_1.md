[Packages](../../README.md) / [Doki](../README.md) / [Doki](README.md) / 

# Filter&lt;T&gt; Class

## Definition

Namespace: [Doki](README.md)

Assembly: [Doki.dll](../README.md)

Package: [Doki](https://www.nuget.org/packages/Doki)

---

**Represents a filter that can be applied to a collection.**

```csharp
public sealed class Filter<T>
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → Filter&lt;T&gt;

## Type Parameters

- `T`
  
   The type of the collection.



## Constructors

|   |Summary|
|---|---|
|Filter(Func&lt;T, System.Boolean&gt;)| Creates a new filter with the given default filter.|


## Properties

|   |Summary|
|---|---|
|Default| Gets the default filter.|
|Expression| Gets or sets the expression to filter the collection.|


