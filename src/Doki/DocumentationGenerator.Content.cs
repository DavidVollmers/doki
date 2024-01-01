using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Doki;

public partial class DocumentationGenerator
{
    private IEnumerable<DocumentationObject> BuildXmlDocumentation(string xpath, XPathNavigator? typeXml,
        DocumentationObject parent)
    {
        var xml = typeXml?.Select(xpath);
        if (xml == null) yield break;

        var navigators = xml.OfType<XPathNavigator>().ToArray();

        foreach (var navigator in navigators)
        {
            yield return BuildXmlDocumentation(navigator, parent);
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
            yield return BuildTypeDocumentationReference(@interface, parent);
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
            yield return BuildTypeDocumentationReference(derivedType, parent);
        }
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private IEnumerable<MemberDocumentation> BuildFieldDocumentation(Type type, DocumentationObject parent,
        XPathNavigator? assemblyXml, ILogger logger)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        foreach (var field in fields)
        {
            var fieldId = $"{parent.Id}.{field.Name}";

            logger.LogDebug("Generating documentation for field {Field}.", fieldId);

            var memberXml = assemblyXml?.SelectSingleNode($"//doc//members//member[@name='M:{fieldId}']");

            var summary = memberXml?.SelectSingleNode("summary");
            if (summary == null)
            {
                logger.LogWarning("No summary found for field {Field}.", fieldId);
            }

            var fieldAssembly = field.DeclaringType!.Assembly.GetName();

            var fieldDocumentation = new MemberDocumentation
            {
                Id = fieldId,
                Name = field.Name,
                Content = DocumentationContent.Field,
                Namespace = field.DeclaringType.Namespace,
                Assembly = fieldAssembly.Name,
                Parent = parent,
            };

            if (summary != null)
                fieldDocumentation.Summary = BuildXmlDocumentation(summary, fieldDocumentation);

            yield return fieldDocumentation;
        }
    }

    private IEnumerable<MemberDocumentation> BuildConstructorDocumentation(Type type, DocumentationObject parent,
        XPathNavigator? assemblyXml, ILogger logger)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        foreach (var constructor in constructors)
        {
            var constructorId = $"{parent.Id}.#ctor";
            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
            {
                constructorId += "(";
                constructorId += string.Join(",",
                    parameters.Select(p =>
                    {
                        var name = p.ParameterType.GetTypeInfo().GetSanitizedName(true);
                        return name.Replace('<', '{').Replace('>', '}');
                    }));
                constructorId += ")";
            }

            logger.LogDebug("Generating documentation for constructor {Constructor}.", constructorId);

            var memberXml = assemblyXml?.SelectSingleNode($"//doc//members//member[@name='M:{constructorId}']");

            var summary = memberXml?.SelectSingleNode("summary");
            if (summary == null)
            {
                logger.LogWarning("No summary found for constructor {Constructor}.", constructorId);
            }

            var constructorAssembly = constructor.DeclaringType!.Assembly.GetName();

            var constructorDocumentation = new MemberDocumentation
            {
                Id = constructorId,
                Name = constructor.GetSanitizedName(),
                Content = DocumentationContent.Constructor,
                Namespace = constructor.DeclaringType.Namespace,
                Assembly = constructorAssembly.Name,
                Parent = parent,
            };

            if (summary != null)
                constructorDocumentation.Summary = BuildXmlDocumentation(summary, constructorDocumentation);

            yield return constructorDocumentation;
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
                            var cref = node.GetAttribute("cref", string.Empty);
                            var href = node.GetAttribute("href", string.Empty);
                            if (cref != string.Empty) items.Add(BuildCRefDocumentation(cref, content));
                            else if (href != string.Empty)
                                items.Add(new Link
                                {
                                    Id = node.BaseURI,
                                    Content = DocumentationContent.Link,
                                    Parent = content,
                                    Url = href,
                                    Text = node.Value.TrimIndentation()
                                });
                            break;
                        case "code":
                            var language = node.GetAttribute("lang", string.Empty);
                            if (language == string.Empty) language = null;
                            items.Add(new CodeBlock
                            {
                                Id = node.BaseURI,
                                Content = DocumentationContent.CodeBlock,
                                Parent = content,
                                Language = language,
                                Code = node.Value.TrimIndentation().TrimEnd()
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
                        Text = node.Value.TrimIndentation()
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported XPathNodeType: " + node.NodeType);
            }
        }

        content.Items = items.ToArray();
        return content;
    }

    private DocumentationObject BuildCRefDocumentation(string cref, DocumentationObject parent)
    {
        if (!cref.StartsWith("T:")) throw new ArgumentOutOfRangeException(nameof(cref), cref, "Unsupported cref.");

        var typeName = cref[2..];
        if (!typeName.Contains('.'))
        {
            var @namespace = parent.TryGetParent<ContentList>(DocumentationContent.Namespace);
            if (@namespace != null) typeName = $"{@namespace.Id}.{typeName}";
        }

        var type = LookupType(typeName);
        if (type == null)
            return new TextContent
            {
                Id = typeName,
                Content = DocumentationContent.Text,
                Parent = parent,
                Text = typeName
            };

        return BuildTypeDocumentationReference(type, parent);
    }

    private TypeDocumentationReference BuildTypeDocumentationReference(Type type, DocumentationObject parent)
    {
        var typeInfo = type.GetTypeInfo();

        var typeId = typeInfo.GetSanitizedName(true, false);

        var assembly = type.Assembly.GetName();

        return new TypeDocumentationReference
        {
            Id = typeId,
            Name = typeInfo.GetSanitizedName(),
            FullName = typeInfo.GetSanitizedName(true),
            Content = DocumentationContent.TypeReference,
            Namespace = type.Namespace,
            Assembly = assembly.Name,
            IsGeneric = type.IsGenericType,
            IsDocumented = IsTypeDocumented(type),
            IsMicrosoft = IsAssemblyFromMicrosoft(assembly),
            Parent = parent
        };
    }
}