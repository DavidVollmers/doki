[Packages](../../../README.md) / [Doki.Output.Abstractions](../../README.md) / [Doki.Output](../README.md) / 

# IOutput Interface

## Definition

Namespace: [Doki.Output](../README.md)

Assembly: [Doki.Output.Abstractions.dll](../../README.md)

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
|WriteAsync(Doki.DocumentationRoot, System.Threading.CancellationToken)| Writes the documentation.|
|WriteAsync(Doki.AssemblyDocumentation, System.Threading.CancellationToken)||
|WriteAsync(Doki.NamespaceDocumentation, System.Threading.CancellationToken)||
|WriteAsync(Doki.TypeDocumentation, System.Threading.CancellationToken)| Writes a specific type documentation.|
|WriteAsync(Doki.MemberDocumentation, System.Threading.CancellationToken)||


