using System.Reflection;

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
}