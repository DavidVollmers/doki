namespace Doki;

public sealed record DocumentationRoot : DocumentationObject
{
    internal AssemblyDocumentation[] InternalAssemblies = [];

    public AssemblyDocumentation[] Assemblies
    {
        get => InternalAssemblies;
        init => InternalAssemblies = value;
    }

    public DocumentationRoot()
    {
        Id = "root";
        ContentType = DocumentationContentType.Root;
    }
}