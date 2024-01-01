namespace Doki;

public sealed class Filter<T>
{
    public Func<T, bool> Default { get; set; }

    public Func<T, bool>? Expression { get; set; }
    
    public Filter(Func<T, bool> defaultFilter)
    {
        Default = defaultFilter;
    }
}