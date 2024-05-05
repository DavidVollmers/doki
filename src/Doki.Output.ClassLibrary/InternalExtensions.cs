namespace Doki.Output.ClassLibrary;

internal static class InternalExtensions
{
    public static string GetTargetFrameworkProperty(this ClassLibraryOutputOptions options)
    {
        return options.TargetFrameworks != null
            ? $"<TargetFrameworks>{string.Join(";", options.TargetFrameworks)}</TargetFrameworks>"
            : $"<TargetFramework>{options.TargetFramework}</TargetFramework>";
    }
}