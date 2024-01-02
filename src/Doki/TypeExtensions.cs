using System.Reflection;
using System.Text;

namespace Doki;

internal static class TypeExtensions
{
    private static readonly Dictionary<string, string> Cache = new();

    public static string GetSanitizedName(this TypeInfo type, bool withNamespace = false, bool parseGenericTypes = true)
    {
        var key = $"{nameof(GetSanitizedName)}:{type.FullName}:{withNamespace}:{parseGenericTypes}";
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var name = (withNamespace ? type.FullName : type.Name) ?? type.Name;

        if (type.IsGenericType)
        {
            if (parseGenericTypes)
            {
                name = name[..name.IndexOf('`')];

                var args = type.GetGenericArguments().Select(a =>
                    a.IsGenericParameter ? a.Name : a.GetTypeInfo().GetSanitizedName(withNamespace));
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

    public static string GetDefinition(this TypeInfo type)
    {
        var key = $"{nameof(GetDefinition)}:{type.FullName}";
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var builder = new StringBuilder();

        if (type.IsPublic) builder.Append("public");
        else if (type.IsNotPublic) builder.Append("internal");
        else if (type.IsNestedPublic) builder.Append("public");
        else if (type.IsNestedPrivate) builder.Append("private");
        else if (type.IsNestedFamily) builder.Append("protected");
        else if (type.IsNestedAssembly) builder.Append("internal");
        else if (type.IsNestedFamANDAssem) builder.Append("private protected");
        else if (type.IsNestedFamORAssem) builder.Append("protected internal");

        switch (type)
        {
            case { IsAbstract: true, IsSealed: true }:
                builder.Append(" static");
                break;
            case { IsAbstract: true, IsInterface: false }:
                builder.Append(" abstract");
                break;
            case { IsSealed: true, IsEnum: false }:
                builder.Append(" sealed");
                break;
        }

        var isRecord = type.IsClass && type.DeclaredProperties.Any(p => p.Name == "EqualityContract");

        if (isRecord) builder.Append(" record");
        else if (type.IsClass) builder.Append(" class");
        else if (type.IsEnum) builder.Append(" enum");
        else if (type.IsInterface) builder.Append(" interface");
        else if (type.IsValueType) builder.Append(" struct");

        builder.Append(' ');
        builder.Append(type.GetSanitizedName());

        var types = new List<string>();

        var interfaces = type.ImplementedInterfaces.ToArray();
        if (type.BaseType != null)
        {
            if (type.BaseType != typeof(object))
                types.Add(type.BaseType.GetTypeInfo().GetSanitizedName(true));

            interfaces = interfaces.Except(type.BaseType.GetTypeInfo().ImplementedInterfaces).ToArray();

            if (isRecord)
            {
                var genericType = typeof(IEquatable<>).MakeGenericType(type.AsType());
                interfaces = interfaces.Except(new[] { genericType }).ToArray();
            }
        }

        if (interfaces.Length != 0)
        {
            types.AddRange(interfaces.Select(i => i.GetTypeInfo().GetSanitizedName(true)));
        }

        if (types.Count != 0)
        {
            builder.Append(" : ");
            builder.Append(string.Join(", ", types));
        }

        Cache.Add(key, builder.ToString());
        return builder.ToString();
    }
}