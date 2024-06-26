﻿namespace Doki.Extensions;

public static class DocumentationObjectExtensions
{
    public static T? TryGetByParents<T>(this DocumentationObject obj, DocumentationContentType? expectedContent = null)
        where T : DocumentationObject
    {
        ArgumentNullException.ThrowIfNull(obj);

        var parent = obj.Parent;
        while (parent != null)
        {
            if (parent is T t && (expectedContent == null || t.ContentType == expectedContent)) return t;

            parent = parent.Parent;
        }

        return null;
    }
}