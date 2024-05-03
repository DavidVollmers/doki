namespace Doki.Output.ClassLibrary;

[DokiOutput("Doki.Output.ClassLibrary")]
public sealed class ClassLibraryOutput(OutputContext context) : OutputBase<ClassLibraryOutputOptions>(context)
{
    public override async Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);
    }

    public override async Task WriteAsync(TypeDocumentation typeDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);
    }
}