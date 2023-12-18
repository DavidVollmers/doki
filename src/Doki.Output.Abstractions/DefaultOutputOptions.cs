namespace Doki.Output;

internal record DefaultOutputOptions : OutputOptions
{
    public DefaultOutputOptions()
    {
        OutputPath = "docs";
    }
}