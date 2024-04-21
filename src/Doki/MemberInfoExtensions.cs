using System.Reflection;

namespace Doki;

internal static class MemberInfoExtensions
{
    public static string[] GetGenericClassParameters(this MemberInfo memberInfo)
    {
        return memberInfo.DeclaringType?.IsGenericType == true
            ? memberInfo.DeclaringType.GetGenericArguments().Select(t => t.Name).ToArray()
            : [];
    }
}