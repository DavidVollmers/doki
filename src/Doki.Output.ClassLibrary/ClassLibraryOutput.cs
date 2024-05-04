namespace Doki.Output.ClassLibrary;

public sealed class ClassLibraryOutput(IOutputOptions<ClassLibraryOutput> options) : IOutput
{
    public async Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);
    }
}