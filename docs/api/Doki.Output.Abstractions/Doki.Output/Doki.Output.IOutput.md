[Packages](../../README.md) / [Doki.Output.Abstractions](../README.md) / [Doki.Output](README.md) / 

# IOutput Interface

## Definition

Namespace: [Doki.Output](README.md)

Assembly: [Doki.Output.Abstractions.dll](../README.md)

Package: [Doki.Output.Abstractions](https://www.nuget.org/packages/Doki.Output.Abstractions)

---

**Interface for writing output.**

```csharp
public interface IOutput
```

## Methods

|   |Summary|
|---|---|
|BeginAsync(System.Threading.CancellationToken)||
|EndAsync(System.Threading.CancellationToken)||
|WriteAsync(Doki.ContentList, System.Threading.CancellationToken)|Writes the content list to the output.|
|WriteAsync(Doki.TypeDocumentation, System.Threading.CancellationToken)|Writes the type documentation to the output.|


