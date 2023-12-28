namespace Doki;

public static class DocumentationObjectExtensions
{
    public static T? TryGetParent<T>(this DocumentationObject obj, DocumentationContent? expectedContent = null)
        where T : DocumentationObject
    {
        ArgumentNullException.ThrowIfNull(obj);

        var parent = obj.Parent;
        while (parent != null)
        {
            if (parent is T t && (expectedContent == null || t.Content == expectedContent)) return t;

            parent = parent.Parent;
        }

        return null;
    }
}