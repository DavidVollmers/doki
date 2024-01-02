using System.Reflection;
using System.Text;

namespace Doki;

internal static class TypeInfoExtensions
{
    private static readonly Dictionary<string, string> Cache = new();

    public static string GetDefinition(this TypeInfo type)
    {
        var key = $"{nameof(GetDefinition)}:{type.GUID}";
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
                types.Add(type.BaseType.GetSanitizedName(true));

            interfaces = interfaces.Except(type.BaseType.GetTypeInfo().ImplementedInterfaces).ToArray();

            if (isRecord)
            {
                var genericType = typeof(IEquatable<>).MakeGenericType(type.AsType());
                interfaces = interfaces.Except(new[] { genericType }).ToArray();
            }
        }

        if (interfaces.Length != 0)
        {
            types.AddRange(interfaces.Select(i => i.GetSanitizedName(true)));
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