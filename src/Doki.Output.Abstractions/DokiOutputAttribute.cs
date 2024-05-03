namespace Doki.Output;

/// <summary>
/// Attribute to mark a class as a Doki output.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DokiOutputAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the output.
    /// </summary>
    public string Name { get; }

    public bool Scoped { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DokiOutputAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    public DokiOutputAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}