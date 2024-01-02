using System.Reflection;

namespace Doki;

internal static class PropertyInfoExtensions
{
    public static string GetXmlDocumentationId(this PropertyInfo propertyInfo)
    {
        var parentId = propertyInfo.DeclaringType?.GetXmlDocumentationId();
        var id = parentId == null ? propertyInfo.Name : parentId + "." + propertyInfo.Name;

        var getParameters = propertyInfo.GetMethod?.GetParameters();
        if (getParameters?.Length > 0)
        {
            return id + ParameterInfoExtensions.GetParametersXmlDocumentationIdString(getParameters,
                propertyInfo.GetGenericClassParameters());
        }

        var setParameters = propertyInfo.SetMethod?.GetParameters();
        if (setParameters?.Length > 1)
        {
            return id + ParameterInfoExtensions.GetParametersXmlDocumentationIdString(
                setParameters.Take(setParameters.Length - 1), propertyInfo.GetGenericClassParameters());
        }

        return id;
    }
}