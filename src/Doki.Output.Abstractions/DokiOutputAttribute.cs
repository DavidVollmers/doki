namespace Doki.Output.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DokiOutputAttribute : Attribute
{
    public string Name { get; }

    public DokiOutputAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}