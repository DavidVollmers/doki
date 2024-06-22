namespace Doki.Extensions;

public static class DocumentationRootExtensions
{
    public static T? TryGetParent<T>(this DocumentationRoot root, DocumentationObject child)
        where T : DocumentationObject
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(child);

        if (child.Parent is T t) return t;

        return SearchParent<T>(root, child, root);
    }

    private static T? SearchParent<T>(DocumentationObject from, DocumentationObject to,
        DocumentationObject subject) where T : DocumentationObject
    {
        if (from == to)
        {
            if (subject is T t) return t;

            return null;
        }

        switch (from)
        {
            case DocumentationRoot root: return SearchParentIn<T>(root.Assemblies, to, root);
            case AssemblyDocumentation assemblyDocumentation:
                return SearchParentIn<T>(assemblyDocumentation.Namespaces, to, assemblyDocumentation);
            case NamespaceDocumentation namespaceDocumentation:
                return SearchParentIn<T>(namespaceDocumentation.Types, to, namespaceDocumentation);
            case TypeDocumentation typeDocumentation:
            {
                var constructor = SearchParentIn<T>(typeDocumentation.Constructors, to, typeDocumentation);
                if (constructor != null) return constructor;

                var method = SearchParentIn<T>(typeDocumentation.Methods, to, typeDocumentation);
                if (method != null) return method;

                var property = SearchParentIn<T>(typeDocumentation.Properties, to, typeDocumentation);
                if (property != null) return property;

                var field = SearchParentIn<T>(typeDocumentation.Fields, to, typeDocumentation);
                if (field != null) return field;

                var interfaceDocumentation = SearchParentIn<T>(typeDocumentation.Interfaces, to, typeDocumentation);
                if (interfaceDocumentation != null) return interfaceDocumentation;

                var derivedType = SearchParentIn<T>(typeDocumentation.DerivedTypes, to, typeDocumentation);
                if (derivedType != null) return derivedType;

                var result = SearchParentInTypeDocumentationReference<T>(typeDocumentation, to);
                if (result != null) return result;

                break;
            }
            case GenericTypeArgumentDocumentation genericTypeArgumentDocumentation:
            {
                if (genericTypeArgumentDocumentation.Description != null)
                {
                    var description = SearchParent<T>(genericTypeArgumentDocumentation.Description, to,
                        genericTypeArgumentDocumentation);
                    if (description != null) return description;
                }

                var result = SearchParentInTypeDocumentationReference<T>(genericTypeArgumentDocumentation, to);
                if (result != null) return result;

                break;
            }
            case TypeDocumentationReference typeDocumentationReference:
            {
                var result = SearchParentInTypeDocumentationReference<T>(typeDocumentationReference, to);
                if (result != null) return result;

                break;
            }
            case MemberDocumentation memberDocumentation:
            {
                var result = SearchParentInMemberDocumentation<T>(memberDocumentation, to);
                if (result != null) return result;

                break;
            }
            case XmlDocumentation xmlDocumentation:
            {
                var result = SearchParentIn<T>(xmlDocumentation.Contents, to, xmlDocumentation);
                if (result != null) return result;

                break;
            }
        }

        return null;
    }

    private static T? SearchParentInTypeDocumentationReference<T>(
        TypeDocumentationReference typeDocumentationReference, DocumentationObject to) where T : DocumentationObject
    {
        if (typeDocumentationReference.BaseType != null)
        {
            var baseType = SearchParent<T>(typeDocumentationReference.BaseType, to, typeDocumentationReference);
            if (baseType != null) return baseType;
        }

        var genericArgument =
            SearchParentIn<T>(typeDocumentationReference.GenericArguments, to, typeDocumentationReference);
        return genericArgument ?? SearchParentInMemberDocumentation<T>(typeDocumentationReference, to);
    }

    private static T? SearchParentInMemberDocumentation<T>(MemberDocumentation memberDocumentation,
        DocumentationObject to) where T : DocumentationObject
    {
        var summary = SearchParentIn<T>(memberDocumentation.Summaries, to, memberDocumentation);
        if (summary != null) return summary;

        var remarks = SearchParentIn<T>(memberDocumentation.Remarks, to, memberDocumentation);
        if (remarks != null) return remarks;

        var example = SearchParentIn<T>(memberDocumentation.Examples, to, memberDocumentation);
        return example;
    }

    private static T? SearchParentIn<T>(IEnumerable<DocumentationObject> children,
        DocumentationObject to, DocumentationObject subject) where T : DocumentationObject
    {
        return children.Select(child => SearchParent<T>(child, to, subject)).FirstOrDefault();
    }
}