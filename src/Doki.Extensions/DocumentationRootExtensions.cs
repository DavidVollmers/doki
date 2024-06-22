namespace Doki.Extensions;

public static class DocumentationRootExtensions
{
    public static DocumentationObject? TryGetParent(this DocumentationRoot root, DocumentationObject child)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(child);

        return child.Parent ?? SearchParent(root, child);
    }

    private static DocumentationObject? SearchParent(DocumentationObject from, DocumentationObject to)
    {
        if (from == to) return from;

        switch (from)
        {
            case DocumentationRoot root: return SearchParentIn(root.Assemblies, to);
            case AssemblyDocumentation assemblyDocumentation:
                return SearchParentIn(assemblyDocumentation.Namespaces, to);
            case NamespaceDocumentation namespaceDocumentation: return SearchParentIn(namespaceDocumentation.Types, to);
            case TypeDocumentation typeDocumentation:
            {
                var constructor = SearchParentIn(typeDocumentation.Constructors, to);
                if (constructor != null) return constructor;

                var method = SearchParentIn(typeDocumentation.Methods, to);
                if (method != null) return method;

                var property = SearchParentIn(typeDocumentation.Properties, to);
                if (property != null) return property;

                var field = SearchParentIn(typeDocumentation.Fields, to);
                if (field != null) return field;

                var interfaceDocumentation = SearchParentIn(typeDocumentation.Interfaces, to);
                if (interfaceDocumentation != null) return interfaceDocumentation;

                var derivedType = SearchParentIn(typeDocumentation.DerivedTypes, to);
                if (derivedType != null) return derivedType;

                var result = SearchParentInTypeDocumentationReference(typeDocumentation, to);
                if (result != null) return result;

                break;
            }
            case GenericTypeArgumentDocumentation genericTypeArgumentDocumentation:
            {
                if (genericTypeArgumentDocumentation.Description != null)
                {
                    var description = SearchParent(genericTypeArgumentDocumentation.Description, to);
                    if (description != null) return description;
                }

                var result = SearchParentInTypeDocumentationReference(genericTypeArgumentDocumentation, to);
                if (result != null) return result;

                break;
            }
            case TypeDocumentationReference typeDocumentationReference:
            {
                var result = SearchParentInTypeDocumentationReference(typeDocumentationReference, to);
                if (result != null) return result;

                break;
            }
            case MemberDocumentation memberDocumentation:
            {
                var result = SearchParentInMemberDocumentation(memberDocumentation, to);
                if (result != null) return result;

                break;
            }
            case XmlDocumentation xmlDocumentation:
            {
                var result = SearchParentIn(xmlDocumentation.Contents, to);
                if (result != null) return result;

                break;
            }
        }

        return null;
    }

    private static DocumentationObject? SearchParentInTypeDocumentationReference(
        TypeDocumentationReference typeDocumentationReference, DocumentationObject to)
    {
        if (typeDocumentationReference.BaseType != null)
        {
            var baseType = SearchParent(typeDocumentationReference.BaseType, to);
            if (baseType != null) return baseType;
        }

        var genericArgument = SearchParentIn(typeDocumentationReference.GenericArguments, to);
        return genericArgument ?? SearchParentInMemberDocumentation(typeDocumentationReference, to);
    }

    private static DocumentationObject? SearchParentInMemberDocumentation(MemberDocumentation memberDocumentation,
        DocumentationObject to)
    {
        var summary = SearchParentIn(memberDocumentation.Summaries, to);
        if (summary != null) return summary;

        var remarks = SearchParentIn(memberDocumentation.Remarks, to);
        if (remarks != null) return remarks;

        var example = SearchParentIn(memberDocumentation.Examples, to);
        return example;
    }

    private static DocumentationObject? SearchParentIn(IEnumerable<DocumentationObject> children,
        DocumentationObject to)
    {
        return children.Select(child => SearchParent(child, to)).OfType<DocumentationObject>().FirstOrDefault();
    }
}