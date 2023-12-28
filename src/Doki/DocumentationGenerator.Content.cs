using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Doki;

public partial class DocumentationGenerator
{
    private IEnumerable<ExampleDocumentation> BuildExampleDocumentation(XPathNavigator? typeXml,
        DocumentationObject parent)
    {
        var examplesXml = typeXml?.Select("example");
        if (examplesXml == null) yield break;

        var examples = examplesXml.OfType<XPathNavigator>().ToArray();

        for (var index = 0; index < examples.Length; index++)
        {
            var example = examples[index];

            var exampleDocumentation = new ExampleDocumentation
            {
                Id = index.ToString(),
                Content = DocumentationContent.Example,
                Parent = parent,
            };

            exampleDocumentation.Documentation = BuildXmlDocumentation(example, exampleDocumentation);

            yield return exampleDocumentation;
        }
    }

    private IEnumerable<GenericTypeArgumentDocumentation> BuildGenericTypeArgumentDocumentation(Type type,
        DocumentationObject parent, XPathNavigator? typeXml, ILogger logger)
    {
        if (!type.IsGenericType) yield break;

        var genericArguments = type.GetGenericArguments();

        foreach (var genericArgument in genericArguments)
        {
            var genericArgumentInfo = genericArgument.GetTypeInfo();

            var genericArgumentId = genericArgument.IsGenericParameter
                ? genericArgument.Name
                : genericArgumentInfo.GetSanitizedName(true, false);

            logger.LogDebug("Generating documentation for generic argument {GenericArgument}.", genericArgumentId);

            var description = typeXml?.SelectSingleNode($"typeparam[@name='{genericArgumentInfo.Name}']");
            if (description == null && typeXml != null)
            {
                logger.LogWarning("No description found for generic argument {GenericArgument}.", genericArgument);
            }

            var genericArgumentAssembly = genericArgument.Assembly.GetName();

            var genericArgumentDocumentation = new GenericTypeArgumentDocumentation
            {
                Id = genericArgumentId,
                Name = genericArgumentInfo.GetSanitizedName(),
                FullName = genericArgumentInfo.GetSanitizedName(true),
                Content = DocumentationContent.GenericTypeArgument,
                Namespace = genericArgument.Namespace,
                Assembly = genericArgumentAssembly.Name,
                IsGeneric = genericArgument.IsGenericType,
                IsGenericParameter = genericArgument.IsGenericParameter,
                IsDocumented = IsTypeDocumented(genericArgument),
                IsMicrosoft = IsAssemblyFromMicrosoft(genericArgumentAssembly),
                Parent = parent,
            };

            if (description != null)
                genericArgumentDocumentation.Description =
                    BuildXmlDocumentation(description, genericArgumentDocumentation);

            yield return genericArgumentDocumentation;
        }
    }

    private IEnumerable<TypeDocumentationReference> BuildInterfaceDocumentation(Type type, DocumentationObject parent)
    {
        var interfaces = type.GetInterfaces().Except(type.BaseType?.GetInterfaces() ?? Array.Empty<Type>())
            .ToArray();

        foreach (var @interface in interfaces)
        {
            var interfaceInfo = @interface.GetTypeInfo();

            var interfaceId = interfaceInfo.GetSanitizedName(true, false);

            var interfaceAssembly = @interface.Assembly.GetName();

            yield return new TypeDocumentationReference
            {
                Id = interfaceId,
                Name = interfaceInfo.GetSanitizedName(),
                FullName = interfaceInfo.GetSanitizedName(true),
                Content = DocumentationContent.TypeReference,
                Namespace = @interface.Namespace,
                Assembly = interfaceAssembly.Name,
                IsGeneric = @interface.IsGenericType,
                IsDocumented = IsTypeDocumented(@interface),
                IsMicrosoft = IsAssemblyFromMicrosoft(interfaceAssembly),
                Parent = parent
            };
        }
    }

    private IEnumerable<TypeDocumentationReference> BuildDerivedTypeDocumentation(Type type, DocumentationObject parent)
    {
        var derivedTypes = new List<Type>();

        foreach (var t in _assemblies.Keys.SelectMany(GetTypesToDocument))
        {
            if (t.BaseType?.FullName == null) continue;

            var baseTypeName = t.BaseType.FullName.Split('[')[0];

            if (baseTypeName == type.FullName) derivedTypes.Add(t);
        }

        foreach (var derivedType in derivedTypes)
        {
            var derivedTypeInfo = derivedType.GetTypeInfo();

            var derivedTypeId = derivedTypeInfo.GetSanitizedName(true, false);

            var derivedTypeAssembly = derivedType.Assembly.GetName();

            yield return new TypeDocumentationReference
            {
                Id = derivedTypeId,
                Name = derivedTypeInfo.GetSanitizedName(),
                FullName = derivedTypeInfo.GetSanitizedName(true),
                Content = DocumentationContent.TypeReference,
                Namespace = derivedType.Namespace,
                Assembly = derivedTypeAssembly.Name,
                IsGeneric = derivedType.IsGenericType,
                IsDocumented = IsTypeDocumented(derivedType),
                IsMicrosoft = IsAssemblyFromMicrosoft(derivedTypeAssembly),
                Parent = parent
            };
        }
    }

    private DocumentationObject BuildXmlDocumentation(XPathNavigator navigator, DocumentationObject parent)
    {
        var content = new ContentList
        {
            Id = navigator.BaseURI,
            Content = DocumentationContent.XmlDocumentation,
            Parent = parent,
            Name = navigator.Name
        };

        var items = new List<DocumentationObject>();
        var nodes = navigator.SelectChildren(XPathNodeType.All);
        while (nodes.MoveNext())
        {
            var node = nodes.Current;
            if (node == null) continue;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (node.NodeType)
            {
                case XPathNodeType.Element:
                    switch (node.Name)
                    {
                        case "see":
                            items.Add(new TypeDocumentationReference
                            {
                                Id = node.BaseURI,
                                Content = DocumentationContent.TypeReference,
                                Parent = content,
                                Name = node.Name,
                                Value = node.GetAttribute("cref", string.Empty)
                            });
                            break;
                        default:
                            items.Add(BuildXmlDocumentation(node, content));
                            break;
                    }

                    break;
                case XPathNodeType.Text:
                    items.Add(new TextContent
                    {
                        Id = node.BaseURI,
                        Content = DocumentationContent.Text,
                        Parent = content,
                        Name = node.Name,
                        Value = node.Value
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported XPathNodeType: " + node.NodeType);
            }
        }

        content.Items = items.ToArray();
        return content;
    }
}