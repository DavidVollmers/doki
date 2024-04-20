namespace Doki;

/// <summary>
/// Represents a filter that can be applied to a collection.
/// </summary>
/// <typeparam name="T">The type of the collection.</typeparam>
public sealed class Filter<T>
{
    /// <summary>
    /// Gets the default filter.
    /// </summary>
    public Func<T, bool> Default { get; }

    /// <summary>
    /// Gets or sets the expression to filter the collection.
    /// </summary>
    public Func<T, bool>? Expression { get; set; }
    
    /// <summary>
    /// Creates a new filter with the given default filter.
    /// </summary>
    /// <param name="defaultFilter">The default filter.</param>
    public Filter(Func<T, bool> defaultFilter)
    {
        Default = defaultFilter;
    }
}