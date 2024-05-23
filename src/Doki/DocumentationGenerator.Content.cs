using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Doki;

public partial class DocumentationGenerator
{
    private const BindingFlags AllMembersBindingFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private IEnumerable<XmlDocumentation> BuildXmlDocumentation(string xpath, XPathNavigator? typeXml,
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
                Namespace = genericArgument.Namespace,
                Assembly = genericArgumentAssembly.Name,
                IsGeneric = genericArgument.IsGenericType,
                IsGenericParameter = genericArgument.IsGenericParameter,
                IsDocumented = IsTypeDocumented(genericArgument),
                IsMicrosoft = IsAssemblyFromMicrosoft(genericArgumentAssembly),
                Parent = parent,
            };

            if (genericArgument.BaseType != null)
                genericArgumentDocumentation.InternalBaseType =
                    BuildTypeDocumentationReference(genericArgument.BaseType, genericArgumentDocumentation);

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
                ContentType = DocumentationContentType.Field,
                Namespace = field.DeclaringType.Namespace,
                Assembly = fieldAssembly.Name,
                Parent = parent,
                IsDocumented = true
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
                ContentType = DocumentationContentType.Constructor,
                Namespace = constructor.DeclaringType.Namespace,
                Assembly = constructorAssembly.Name,
                Parent = parent,
                IsDocumented = true
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
                ContentType = DocumentationContentType.Property,
                Namespace = property.DeclaringType.Namespace,
                Assembly = propertyAssembly.Name,
                Parent = parent,
                IsDocumented = true
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
                ContentType = DocumentationContentType.Method,
                Namespace = method.DeclaringType.Namespace,
                Assembly = methodAssembly.Name,
                Parent = parent,
                IsDocumented = true
            };

            if (summary != null)
                methodDocumentation.Summary = BuildXmlDocumentation(summary, methodDocumentation);

            yield return methodDocumentation;
        }
    }

    private XmlDocumentation BuildXmlDocumentation(XPathNavigator navigator, DocumentationObject parent)
    {
        var content = new XmlDocumentation
        {
            Id = navigator.Name,
            Parent = parent,
            Name = navigator.LocalName
        };

        var contents = new List<DocumentationObject>();
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
                            if (cref != string.Empty) contents.Add(BuildCRefDocumentation(cref, content));
                            else if (href != string.Empty)
                                contents.Add(new Link
                                {
                                    Id = node.Name,
                                    Parent = content,
                                    Url = href,
                                    Text = node.Value.TrimIndentation()
                                });
                            break;
                        case "code":
                            var language = node.GetAttribute("lang", string.Empty);
                            if (language == string.Empty) language = null;
                            contents.Add(new CodeBlock
                            {
                                Id = node.Name,
                                Parent = content,
                                Language = language,
                                Code = node.Value.TrimIndentation().TrimEnd()
                            });
                            break;
                        default:
                            contents.Add(BuildXmlDocumentation(node, content));
                            break;
                    }

                    break;
                case XPathNodeType.Text:
                    contents.Add(new TextContent
                    {
                        Id = "text",
                        Parent = content,
                        Text = node.Value.TrimIndentation()
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported XPathNodeType: " + node.NodeType);
            }
        }

        content.InternalContents = contents.ToArray();
        return content;
    }

    private DocumentationObject BuildCRefDocumentation(string cref, DocumentationObject parent)
    {
        var parts = cref.Split(':');
        if (parts.Length != 2) throw new ArgumentOutOfRangeException(nameof(cref), cref, "Invalid cref format.");

        var memberType = parts[0];
        var memberName = parts[1];
        var typeName = parts[1];
        if (memberType != "T")
        {
            memberName = memberName.Split('.').Last();
            typeName = typeName[..typeName.LastIndexOf('.')];
        }

        var type = LookupType(typeName);
        if (type == null)
            return new TextContent
            {
                Id = "text",
                Parent = parent,
                Text = memberName
            };

        var typeDocumentationReference = BuildTypeDocumentationReference(type, parent);

        switch (memberType)
        {
            case "T":
                return typeDocumentationReference;
            case "P":
            {
                var property = type.GetProperty(memberName, AllMembersBindingFlags);
                if (property == null)
                    return new TextContent
                    {
                        Id = "text",
                        Parent = parent,
                        Text = memberName
                    };

                return new MemberDocumentation
                {
                    Id = property.GetXmlDocumentationId(),
                    Name = property.Name,
                    ContentType = DocumentationContentType.Property,
                    Namespace = type.Namespace,
                    Assembly = type.Assembly.GetName().Name,
                    Parent = typeDocumentationReference,
                    IsDocumented = PropertyFilter.Expression?.Invoke(property) ?? PropertyFilter.Default(property)
                };
            }
            case "F":
            {
                var field = type.GetField(memberName, AllMembersBindingFlags);
                if (field == null)
                    return new TextContent
                    {
                        Id = "text",
                        Parent = parent,
                        Text = memberName
                    };

                return new MemberDocumentation
                {
                    Id = field.GetXmlDocumentationId(),
                    Name = field.Name,
                    ContentType = DocumentationContentType.Field,
                    Namespace = type.Namespace,
                    Assembly = type.Assembly.GetName().Name,
                    Parent = typeDocumentationReference,
                    IsDocumented = FieldFilter.Expression?.Invoke(field) ?? FieldFilter.Default(field)
                };
            }
            case "M":
            {
                if (memberName.StartsWith("#ctor"))
                {
                    var constructors = type.GetConstructors(AllMembersBindingFlags);

                    var constructor = constructors.FirstOrDefault(c => c.GetXmlDocumentationId() == parts[1]);

                    if (constructor == null)
                        return new TextContent
                        {
                            Id = "text",
                            Parent = parent,
                            Text = memberName
                        };

                    return new MemberDocumentation
                    {
                        Id = constructor.GetXmlDocumentationId(),
                        Name = constructor.GetSanitizedName(),
                        ContentType = DocumentationContentType.Constructor,
                        Namespace = type.Namespace,
                        Assembly = type.Assembly.GetName().Name,
                        Parent = typeDocumentationReference,
                        IsDocumented = ConstructorFilter.Expression?.Invoke(constructor) ??
                                       ConstructorFilter.Default(constructor)
                    };
                }

                var method = type.GetMethod(memberName, AllMembersBindingFlags);

                if (method == null)
                    return new TextContent
                    {
                        Id = "text",
                        Parent = parent,
                        Text = memberName
                    };

                return new MemberDocumentation
                {
                    Id = method.GetXmlDocumentationId(),
                    Name = method.GetSanitizedName(),
                    ContentType = DocumentationContentType.Method,
                    Namespace = type.Namespace,
                    Assembly = type.Assembly.GetName().Name,
                    Parent = typeDocumentationReference,
                    IsDocumented = MethodFilter.Expression?.Invoke(method) ?? MethodFilter.Default(method)
                };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(cref), cref, "Unsupported cref member type.");
        }
    }

    private TypeDocumentationReference BuildTypeDocumentationReference(Type type, DocumentationObject parent)
    {
        var typeId = type.GetXmlDocumentationId();

        var assembly = type.Assembly.GetName();

        var typeDocumentationReference = new TypeDocumentationReference
        {
            Id = typeId,
            Name = type.GetSanitizedName(),
            FullName = type.GetSanitizedName(true),
            ContentType = DocumentationContentType.TypeReference,
            Namespace = type.Namespace,
            Assembly = assembly.Name,
            IsGeneric = type.IsGenericType,
            IsDocumented = IsTypeDocumented(type),
            IsMicrosoft = IsAssemblyFromMicrosoft(assembly),
            Parent = parent
        };

        if (type.BaseType != null)
            typeDocumentationReference.InternalBaseType =
                BuildTypeDocumentationReference(type.BaseType, typeDocumentationReference);

        return typeDocumentationReference;
    }
}