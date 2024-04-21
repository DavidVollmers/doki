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
|OutputBase(Doki.Output.OutputContext)|Initializes a new instance of the [OutputBase&lt;T&gt;](Doki.Output.OutputBase_1.md) class.|


## Properties

|   |Summary|
|---|---|
|Options|Gets the options for the output.|
|Context|Gets the output context.|
|OutputDirectory|Gets the output directory.|


## Methods

|   |Summary|
|---|---|
|WriteAsync(Doki.ContentList, System.Threading.CancellationToken)|Writes the content list to the output.|
|WriteAsync(Doki.TypeDocumentation, System.Threading.CancellationToken)|Writes the type documentation to the output.|


