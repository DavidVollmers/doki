namespace Doki.Output;

public static class OutputOptionsExtensions
{
    public static void ClearOutputDirectoryIfRequired<T>(this OutputOptions<T> options) where T : IOutput
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options is { ClearOutput: true, OutputDirectory.Exists: true })
        {
            options.OutputDirectory.Delete(true);
        }
    }
}