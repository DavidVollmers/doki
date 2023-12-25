namespace Doki.Output;

public interface IOutput
{
    Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default);

    Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default);
}