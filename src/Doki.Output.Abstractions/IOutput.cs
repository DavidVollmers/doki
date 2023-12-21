namespace Doki.Output;

public interface IOutput
{
    Task WriteAsync(TableOfContents tableOfContents, CancellationToken cancellationToken = default);

    Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default);
}