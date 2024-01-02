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
                        a.IsGenericParameter ? a.Name : a.GetTypeInfo().GetSanitizedName(true)));
                    name += ">";
                }

                break;
            }
        }

        var parameters = methodBase.GetParameters();
        if (parameters.Length <= 0) return name + "()";

        name += "(";
        name += string.Join(", ", parameters.Select(p => p.ParameterType.GetTypeInfo().GetSanitizedName(true)));
        name += ")";

        return name;
    }
}