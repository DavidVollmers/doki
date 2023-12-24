using System.Reflection;
using System.Text;

namespace Doki;

internal static class TypeExtensions
{
    private static readonly Dictionary<string, string> NameCache = new();
    private static readonly Dictionary<string, string> FullNameCache = new();
    private static readonly Dictionary<string, string> DefinitionCache = new();

    public static string GetSanitizedName(this TypeInfo type, bool withNamespace = false)
    {
        if (!withNamespace && NameCache.TryGetValue(type.FullName!, out var cached)) return cached;
        if (withNamespace && FullNameCache.TryGetValue(type.FullName!, out cached)) return cached;

        var name = withNamespace ? type.FullName! : type.Name;
        // ReSharper disable once InvertIf
        if (type.IsGenericType)
        {
            var index = name.LastIndexOf('`');
            name = name[..index];

            var args = type.GenericTypeParameters.Select(p => p.Name).ToList();
            args.AddRange(type.GenericTypeArguments.Select(a => a.GetTypeInfo().GetSanitizedName(withNamespace)));
            name += $"<{string.Join(", ", args)}>";
        }

        if (withNamespace) FullNameCache.Add(type.FullName!, name);
        else NameCache.Add(type.FullName!, name);
        return name;
    }

    public static string GetDefinition(this TypeInfo type)
    {
        if (DefinitionCache.TryGetValue(type.FullName!, out var cached)) return cached;

        var builder = new StringBuilder();

        if (type.IsPublic) builder.Append("public");
        else if (type.IsNotPublic) builder.Append("internal");
        else if (type.IsNestedPublic) builder.Append("public");
        else if (type.IsNestedPrivate) builder.Append("private");
        else if (type.IsNestedFamily) builder.Append("protected");
        else if (type.IsNestedAssembly) builder.Append("internal");
        else if (type.IsNestedFamANDAssem) builder.Append("private protected");
        else if (type.IsNestedFamORAssem) builder.Append("protected internal");

        if (type is {IsAbstract: true, IsSealed: true}) builder.Append(" static");
        else if (type.IsAbstract) builder.Append(" abstract");
        else if (type.IsSealed) builder.Append(" sealed");

        if (type.IsClass) builder.Append(" class");
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

        DefinitionCache.Add(type.FullName!, builder.ToString());
        return builder.ToString();
    }
}