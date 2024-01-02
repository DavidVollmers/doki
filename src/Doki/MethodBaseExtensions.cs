using System.Reflection;

namespace Doki;

internal static class MethodBaseExtensions
{
    public static string GetSanitizedName(this MethodBase methodBase)
    {
        var name = methodBase.Name;
        switch (methodBase)
        {
            case ConstructorInfo:
                name = methodBase.DeclaringType?.Name ?? name;
                if (methodBase.DeclaringType?.IsGenericType == true)
                {
                    name = name[..name.IndexOf('`')];
                }

                break;
            case MethodInfo methodInfo:
            {
                if (methodInfo.IsGenericMethod)
                {
                    name = name[..name.IndexOf('`')];

                    name += "<";
                    name += string.Join(", ", methodInfo.GetGenericArguments().Select(a =>
                        a.IsGenericParameter ? a.Name : a.GetSanitizedName(true)));
                    name += ">";
                }

                break;
            }
        }

        var parameters = methodBase.GetParameters();
        if (parameters.Length <= 0) return name + "()";

        name += "(";
        name += string.Join(", ", parameters.Select(p => p.ParameterType.GetSanitizedName(true)));
        name += ")";

        return name;
    }

    public static string GetXmlDocumentationId(this MethodBase methodBase)
    {
        var parentId = methodBase.DeclaringType?.GetXmlDocumentationId();
        var methodId =
            $"{methodBase.GetShortMethodName()}{ParameterInfoExtensions.GetParametersXmlDocumentationIdString(methodBase.GetParameters(), methodBase.GetGenericClassParameters())}{methodBase.GetExplicitImplicitSuffix()}";
        return parentId == null ? methodId : parentId + "." + methodId;
    }

    private static string GetShortMethodName(this MethodBase methodBase)
    {
        if (methodBase.IsConstructor) return "#ctor";
        return (methodBase.IsIndexerProperty() ? "Item" : methodBase.Name) +
               (methodBase.IsGenericMethod ? "``" + methodBase.GetGenericArguments().Length : null);
    }

    private static bool IsIndexerProperty(this MethodBase methodBase)
    {
        return methodBase is { IsSpecialName: true, Name: "get_Item" or "set_Item" };
    }

    private static string? GetExplicitImplicitSuffix(this MethodBase methodBase)
    {
        if (!methodBase.IsSpecialName ||
            (methodBase.Name != "op_Explicit" && methodBase.Name != "op_Implicit")) return null;
        if (methodBase is not MethodInfo methodInfo) return null;
        return "~" + methodInfo.ReturnType.GetXmlDocumentationId();
    }
}