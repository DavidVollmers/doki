namespace Doki.TestAssembly;

/// <summary>
/// Use <see cref="ClassWithCRefs()"/> to create a new instance.
/// </summary>
public class ClassWithCRefs
{
    /// <summary>
    /// Is used in <see cref="Method"/>.
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Does something with <see cref="Property"/>.
    /// </summary>
    public void Method()
    {
        var property = Property;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ClassWithCRefs"/>.
    /// </summary>
    public ClassWithCRefs()
    {
    }
}