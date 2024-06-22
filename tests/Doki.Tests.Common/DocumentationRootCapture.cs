using Doki.Output;

namespace Doki.Tests.Common;

public sealed class DocumentationRootCapture : IOutput
{
    public DocumentationRoot? Root { get; private set; }

    public Task WriteAsync(DocumentationRoot root, CancellationToken cancellationToken = default)
    {
        Root = root;
        return Task.CompletedTask;
    }
}