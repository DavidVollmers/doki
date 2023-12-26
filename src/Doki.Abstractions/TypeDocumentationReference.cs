﻿namespace Doki;

public record TypeDocumentationReference : TypeDocumentationObject
{
    public string Name { get; internal init; } = null!;

    public string FullName { get; internal init; } = null!;

    public string Definition { get; internal init; } = null!;
    
    public bool IsDocumented { get; internal init; }

    public bool IsMicrosoft { get; internal init; }

    public TypeDocumentationReference? BaseType { get; internal set; }
    
    public TypeDocumentationReference()
    {
        Content = DocumentationContent.TypeReference;
    }

    public TypeDocumentationReference(TypeDocumentationReference reference) : base(reference)
    {
        Content = DocumentationContent.TypeReference;
        Name = reference.Name;
        FullName = reference.FullName;
        Definition = reference.Definition;
        IsDocumented = reference.IsDocumented;
        IsMicrosoft = reference.IsMicrosoft;
        BaseType = reference.BaseType;
    }
}