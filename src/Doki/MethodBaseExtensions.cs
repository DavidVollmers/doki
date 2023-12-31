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
                break;
            case MethodInfo methodInfo:
            {
                if (methodInfo.IsGenericMethod)
                {
                    name += "<";
                    name += string.Join(", ", methodInfo.GetGenericArguments().Select(a =>
                        a.IsGenericParameter ? a.Name : a.GetTypeInfo().GetSanitizedName(true)));
                    name += ">";
                }

                break;
            }
        }

        return name;
    }
}