using System.Reflection;
using System.Text;

namespace Doki;

internal static class TypeExtensions
{
    public static string GetSanitizedName(this TypeInfo type, bool withNamespace = false)
    {
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

        return name;
    }

    public static string GetDefinition(this TypeInfo type)
    {
        var builder = new StringBuilder();

        if (type.IsPublic) builder.Append("public");
        else if (type.IsNotPublic) builder.Append("internal");
        else if (type.IsNestedPublic) builder.Append("public");
        else if (type.IsNestedPrivate) builder.Append("private");
        else if (type.IsNestedFamily) builder.Append("protected");
        else if (type.IsNestedAssembly) builder.Append("internal");
        else if (type.IsNestedFamANDAssem) builder.Append("private protected");
        else if (type.IsNestedFamORAssem) builder.Append("protected internal");

        if (type is { IsAbstract: true, IsSealed: true }) builder.Append(" static");
        else if (type.IsAbstract) builder.Append(" abstract");
        else if (type.IsSealed) builder.Append(" sealed");

        if (type.IsClass) builder.Append(" class");
        else if (type.IsEnum) builder.Append(" enum");
        else if (type.IsInterface) builder.Append(" interface");
        else if (type.IsValueType) builder.Append(" struct");

        builder.Append(' ');
        builder.Append(type.GetSanitizedName());

        var types = new List<string>();

        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            types.Add(type.BaseType.GetTypeInfo().GetSanitizedName(true));
        }

        if (type.ImplementedInterfaces.Any())
        {
            types.AddRange(type.ImplementedInterfaces.Select(i => i.GetTypeInfo().GetSanitizedName(true)));
        }

        // ReSharper disable once InvertIf
        if (types.Any())
        {
            builder.Append(" : ");
            builder.Append(string.Join(", ", types));
        }

        return builder.ToString();
    }
}