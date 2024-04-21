namespace Doki.Output;

/// <summary>
/// Interface for writing output.
/// </summary>
public interface IOutput
{
    /// <summary>
    /// Writes the content list to the output.
    /// </summary>
    /// <param name="contentList">The content list to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the type documentation to the output.
    /// </summary>
    /// <param name="typeDocumentation">The type documentation to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default);
}