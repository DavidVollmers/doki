namespace Doki.Output.ClassLibrary;

public sealed class ClassLibraryOutput(ClassLibraryOutputOptions options) : IOutput
{
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        options.ClearOutputDirectoryIfRequired();

        return Task.CompletedTask;
    }

    public async Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);
    }
}