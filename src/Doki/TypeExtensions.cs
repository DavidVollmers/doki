using System.Reflection;
using System.Text.RegularExpressions;

namespace Doki;

internal static class TypeExtensions
{
    private static readonly Dictionary<string, string> Cache = new();

    public static string GetSanitizedName(this Type type, bool withNamespace = false, bool parseGenericTypes = true)
    {
        var key =
            $"{nameof(GetSanitizedName)}:{type.GUID}:{{type.FullName ?? type.Name}}:{withNamespace}:{parseGenericTypes}";
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var name = (withNamespace ? type.FullName : type.Name) ?? type.Name;

        if (type.IsGenericType)
        {
            if (parseGenericTypes)
            {
                name = name[..name.IndexOf('`')];

                var args = type.GetGenericArguments().Select(a =>
                    a.IsGenericParameter ? a.Name : a.GetSanitizedName(withNamespace));
                name += $"<{string.Join(", ", args)}>";
            }
            else
            {
                var index = name.IndexOf('[');
                if (index != -1) name = name[..index];
            }
        }

        Cache.Add(key, name);
        return name;
    }

    public static string GetXmlDocumentationId(this Type type)
    {
        return GetXmlDocumentationIdCore(type);
    }

    internal static string GetXmlDocumentationIdCore(this Type type, bool isOut = false,
        bool isMethodParameter = false, string[]? genericClassParams = null)
    {
        var key =
            $"{nameof(GetXmlDocumentationIdCore)}:{type.GUID}:{type.FullName ?? type.Name}:{isOut}:{isMethodParameter}:{genericClassParams}";
        if (Cache.TryGetValue(key, out var cached)) return cached;

        if (type.IsGenericParameter)
            return $"{GetGenericParameterPrefix(type, genericClassParams)}{type.GenericParameterPosition}";

        string id;

        var args = type.GetGenericArguments();
        var @namespace = type.Namespace == null ? null : $"{type.Namespace}.";
        var suffix = isOut ? "@" : null;

        if (type is { MemberType: MemberTypes.TypeInfo, IsGenericTypeDefinition: false } &&
            (type.IsGenericType || args.Length > 0) && (!type.IsClass || isMethodParameter))
        {
            var parameters = string.Join(",",
                args.Select(a => a.GetXmlDocumentationIdCore(false, isMethodParameter, genericClassParams)));
            var name = Regex.Replace(type.Name, "`[0-9]+", $"{{{parameters}}}");
            id = $"{@namespace}{name}{suffix}";
        }
        else if (type.IsNested)
        {
            id = $"{@namespace}{type.DeclaringType!.Name}.{type.Name}{suffix}";
        }
        else if (type.ContainsGenericParameters && (type.IsArray || type.GetElementType() != null))
        {
            var typeName = type.GetElementType()
                !.GetXmlDocumentationIdCore(false, isMethodParameter, genericClassParams);
            id = $"{typeName}{(type.IsArray ? "[]" : null)}{suffix}";
        }
        else
        {
            id = $"{@namespace}{type.Name}{suffix}";
        }

        id = id.Replace("&", string.Empty);

        while (id.Contains("[,"))
        {
            var index = id.IndexOf("[,", StringComparison.Ordinal);
            var lastIndex = id.IndexOf(']', index);
            id = string.Concat(id.AsSpan(0, index + 1), string.Join(",", Enumerable.Repeat("0:", lastIndex - index)),
                id.AsSpan(lastIndex));
        }

        Cache.Add(key, id);
        return id;
    }

    private static string GetGenericParameterPrefix(MemberInfo member, string[]? genericClassParams)
    {
        return genericClassParams != null && genericClassParams.Contains(member.Name) ? "`" : "``";
    }
}