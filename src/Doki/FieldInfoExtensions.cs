using System.Reflection;

namespace Doki;

internal static class FieldInfoExtensions
{
    public static string GetXmlDocumentationId(this FieldInfo fieldInfo)
    {
        var parentId = fieldInfo.DeclaringType?.GetXmlDocumentationId();
        return parentId == null ? fieldInfo.Name : parentId + "." + fieldInfo.Name;
    }
}