[Packages](../../README.md) / [Doki.Output.Abstractions](../README.md) / [Doki.Output](README.md) / 

# OutputContext Class

## Definition

Namespace: [Doki.Output](README.md)

Assembly: [Doki.Output.Abstractions.dll](../README.md)

Package: [Doki.Output.Abstractions](https://www.nuget.org/packages/Doki.Output.Abstractions)

---

**The context in which the output is being generated.**

```csharp
public sealed record OutputContext
```

Inheritance: [System.Object](https://learn.microsoft.com/en-us/dotnet/api/System.Object) → OutputContext

Implements: [System.IEquatable&lt;Doki.Output.OutputContext&gt;](https://learn.microsoft.com/en-us/dotnet/api/System.IEquatable&lt;Doki.Output.OutputContext&gt;)

## Constructors

|   |Summary|
|---|---|
|OutputContext(System.IO.DirectoryInfo, System.Nullable&lt;System.Text.Json.JsonElement&gt;)|The context in which the output is being generated.|


## Properties

|   |Summary|
|---|---|
|WorkingDirectory|The directory in which the output is being generated.|
|Options|The JSON serialized options to use when generating the output.|


