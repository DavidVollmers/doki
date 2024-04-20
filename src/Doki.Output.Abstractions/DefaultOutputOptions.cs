namespace Doki.Output;

public sealed record DefaultOutputOptions : OutputOptions
{
    public DefaultOutputOptions()
    {
        OutputPath = "docs";
    }
}