namespace Doki.Extensions;

public static class DocumentationObjectExtensions
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

                var example = SearchParentIn(typeDocumentation.Examples, to);
                if (example != null) return example;

                var remarks = SearchParentIn(typeDocumentation.Remarks, to);
                if (remarks != null) return remarks;

                var interfaceDocumentation = SearchParentIn(typeDocumentation.Interfaces, to);
                if (interfaceDocumentation != null) return interfaceDocumentation;

                var derivedType = SearchParentIn(typeDocumentation.DerivedTypes, to);
                if (derivedType != null) return derivedType;

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
        var summary = SearchParentIn(memberDocumentation.Summary, to);
        return summary ?? SearchParentIn(memberDocumentation.Remarks, to);
    }

    private static DocumentationObject? SearchParentIn(IEnumerable<DocumentationObject> children,
        DocumentationObject to)
    {
        return children.Select(child => SearchParent(child, to)).OfType<DocumentationObject>().FirstOrDefault();
    }
}