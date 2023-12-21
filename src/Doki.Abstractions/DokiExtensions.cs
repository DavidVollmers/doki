namespace Doki;

public static class DokiExtensions
{
    public static T? TryGetParent<T>(this DokiElement element, DokiContent? expectedContent = null)
        where T : DokiElement
    {
        ArgumentNullException.ThrowIfNull(element);

        var parent = element.Parent;
        while (parent != null)
        {
            if (parent is T t && (expectedContent == null || t.Content == expectedContent)) return t;

            parent = parent.Parent;
        }

        return null;
    }
}