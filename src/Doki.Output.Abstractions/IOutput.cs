namespace Doki.Output;

/// <summary>
/// Interface for writing output.
/// </summary>
public interface IOutput
{
    Task BeginAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    Task EndAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Writes the documentation.
    /// </summary>
    /// <param name="root">The documentation to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteAsync(DocumentationRoot root, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a specific type documentation.
    /// </summary>
    /// <param name="typeDocumentation">The type documentation to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}