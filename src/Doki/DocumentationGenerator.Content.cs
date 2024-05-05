using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Doki;

public partial class DocumentationGenerator
{
    private const BindingFlags AllMembersBindingFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

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
            var genericArgumentId = genericArgument.IsGenericParameter
                ? genericArgument.Name
                : genericArgument.GetXmlDocumentationId();

            logger.LogDebug("Generating documentation for generic argument {GenericArgument}.", genericArgumentId);

            var description = typeXml?.SelectSingleNode($"typeparam[@name='{genericArgumentId}']");
            if (description == null && typeXml != null)
            {
                logger.LogWarning("No description found for generic argument {GenericArgument}.", genericArgument);
            }

            var genericArgumentAssembly = genericArgument.Assembly.GetName();

            var genericArgumentDocumentation = new GenericTypeArgumentDocumentation
            {
                Id = genericArgumentId,
                Name = genericArgument.GetSanitizedName(),
                FullName = genericArgument.GetSanitizedName(true),
                Content = DocumentationContentType.GenericTypeArgument,
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
        var interfaces = type.GetInterfaces().Except(type.BaseType?.GetInterfaces() ?? []).ToArray();

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

    private IEnumerable<MemberDocumentation> BuildFieldDocumentation(Type type, DocumentationObject parent,
        XPathNavigator? assemblyXml, ILogger logger)
    {
        var fields = type.GetFields(AllMembersBindingFlags).Where(FieldFilter.Expression ?? FieldFilter.Default);

        foreach (var field in fields)
        {
            if (!IncludeInheritedMembers && field.DeclaringType != type) continue;

            var fieldId = field.GetXmlDocumentationId();

            logger.LogDebug("Generating documentation for field {Field}.", fieldId);

            var memberXml = assemblyXml?.SelectSingleNode($"//doc//members//member[@name='F:{fieldId}']");

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
                Content = DocumentationContentType.Field,
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
        var constructors = type.GetConstructors(AllMembersBindingFlags)
            .Where(ConstructorFilter.Expression ?? ConstructorFilter.Default);

        foreach (var constructor in constructors)
        {
            if (!IncludeInheritedMembers && constructor.DeclaringType != type) continue;

            var constructorId = constructor.GetXmlDocumentationId();

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
                Content = DocumentationContentType.Constructor,
                Namespace = constructor.DeclaringType.Namespace,
                Assembly = constructorAssembly.Name,
                Parent = parent,
            };

            if (summary != null)
                constructorDocumentation.Summary = BuildXmlDocumentation(summary, constructorDocumentation);

            yield return constructorDocumentation;
        }
    }

    private IEnumerable<MemberDocumentation> BuildPropertyDocumentation(Type type, DocumentationObject parent,
        XPathNavigator? assemblyXml, ILogger logger)
    {
        var properties = type.GetProperties(AllMembersBindingFlags)
            .Where(PropertyFilter.Expression ?? PropertyFilter.Default);

        foreach (var property in properties)
        {
            if (!IncludeInheritedMembers && property.DeclaringType != type) continue;

            var propertyId = property.GetXmlDocumentationId();

            logger.LogDebug("Generating documentation for property {Property}.", propertyId);

            var memberXml = assemblyXml?.SelectSingleNode($"//doc//members//member[@name='P:{propertyId}']");

            var summary = memberXml?.SelectSingleNode("summary");
            if (summary == null)
            {
                logger.LogWarning("No summary found for property {Property}.", propertyId);
            }

            var propertyAssembly = property.DeclaringType!.Assembly.GetName();

            var propertyDocumentation = new MemberDocumentation
            {
                Id = propertyId,
                Name = property.Name,
                Content = DocumentationContentType.Property,
                Namespace = property.DeclaringType.Namespace,
                Assembly = propertyAssembly.Name,
                Parent = parent,
            };

            if (summary != null)
                propertyDocumentation.Summary = BuildXmlDocumentation(summary, propertyDocumentation);

            yield return propertyDocumentation;
        }
    }

    private IEnumerable<MemberDocumentation> BuildMethodDocumentation(Type type, DocumentationObject parent,
        XPathNavigator? assemblyXml, ILogger logger)
    {
        var methods = type.GetMethods(AllMembersBindingFlags).Where(MethodFilter.Expression ?? MethodFilter.Default);

        foreach (var method in methods)
        {
            if (!IncludeInheritedMembers && method.DeclaringType != type) continue;

            var methodId = method.GetXmlDocumentationId();

            //TODO filter out all methods that are generated by the compiler
            if (methodId.Contains(">$")) continue;

            logger.LogDebug("Generating documentation for method {Method}.", methodId);

            var memberXml = assemblyXml?.SelectSingleNode($"//doc//members//member[@name='M:{methodId}']");

            var summary = memberXml?.SelectSingleNode("summary");
            if (summary == null)
            {
                logger.LogWarning("No summary found for method {Method}.", methodId);
            }

            var methodAssembly = method.DeclaringType!.Assembly.GetName();

            var methodDocumentation = new MemberDocumentation
            {
                Id = methodId,
                Name = method.GetSanitizedName(),
                Content = DocumentationContentType.Property,
                Namespace = method.DeclaringType.Namespace,
                Assembly = methodAssembly.Name,
                Parent = parent,
            };

            if (summary != null)
                methodDocumentation.Summary = BuildXmlDocumentation(summary, methodDocumentation);

            yield return methodDocumentation;
        }
    }

    private DocumentationObject BuildXmlDocumentation(XPathNavigator navigator, DocumentationObject parent)
    {
        var content = new ContentList
        {
            Id = navigator.BaseURI,
            Content = DocumentationContentType.XmlDocumentation,
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
                                    Content = DocumentationContentType.Link,
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
                                Content = DocumentationContentType.CodeBlock,
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
                        Content = DocumentationContentType.Text,
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
            var @namespace = parent.TryGetParent<ContentList>(DocumentationContentType.Namespace);
            if (@namespace != null) typeName = $"{@namespace.Id}.{typeName}";
        }

        var type = LookupType(typeName);
        if (type == null)
            return new TextContent
            {
                Id = typeName,
                Content = DocumentationContentType.Text,
                Parent = parent,
                Text = typeName
            };

        return BuildTypeDocumentationReference(type, parent);
    }

    private TypeDocumentationReference BuildTypeDocumentationReference(Type type, DocumentationObject parent)
    {
        var typeId = type.GetXmlDocumentationId();

        var assembly = type.Assembly.GetName();

        return new TypeDocumentationReference
        {
            Id = typeId,
            Name = type.GetSanitizedName(),
            FullName = type.GetSanitizedName(true),
            Content = DocumentationContentType.TypeReference,
            Namespace = type.Namespace,
            Assembly = assembly.Name,
            IsGeneric = type.IsGenericType,
            IsDocumented = IsTypeDocumented(type),
            IsMicrosoft = IsAssemblyFromMicrosoft(assembly),
            Parent = parent
        };
    }
}