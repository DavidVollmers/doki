namespace Doki.Output;

/// <summary>
/// Default output options.
/// </summary>
public sealed record DefaultOutputOptions : OutputOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultOutputOptions"/> class.
    /// </summary>
    public DefaultOutputOptions()
    {
        OutputPath = "docs";
    }
}