[Packages](../../README.md) / [Doki.Output.Abstractions](../README.md) / [Doki.Output](README.md) / 

# OutputBase&lt;T&gt; Class

## Definition

Namespace: [Doki.Output](README.md)

Assembly: [Doki.Output.Abstractions.dll](../README.md)

Package: [Doki.Output.Abstractions](https://www.nuget.org/packages/Doki.Output.Abstractions)

---

**Base class for file based outputs.**

```csharp
public abstract class OutputBase<T> : Doki.Output.IOutput
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → OutputBase&lt;T&gt;

Derived: [MarkdownOutput](../../Doki.Output.Markdown/Doki.Output.Markdown/Doki.Output.Markdown.MarkdownOutput.md)

Implements: [IOutput](Doki.Output.IOutput.md)

## Type Parameters

- `T`
  
  The type of options for the output.



## Constructors

|   |Summary|
|---|---|
|OutputBase(Doki.Output.OutputContext)||


## Properties

|   |Summary|
|---|---|
|Options||
|Context||
|OutputDirectory||


## Methods

|   |Summary|
|---|---|
|WriteAsync(Doki.ContentList, System.Threading.CancellationToken)||
|WriteAsync(Doki.TypeDocumentation, System.Threading.CancellationToken)||


