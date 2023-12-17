namespace Doki.Output;

internal record DefaultOutputOptions : OutputOptions
{
    public DefaultOutputOptions()
    {
        OutputDirectory = "docs";
    }
}