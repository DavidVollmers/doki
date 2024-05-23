namespace Doki.TestAssembly;

public class ClassWithPropertyRef
{
    public string? Property { get; set; }

    /// <summary>
    /// Does something with <see cref="Property"/>.
    /// </summary>
    public void Method()
    {
        var property = Property;
    }
}