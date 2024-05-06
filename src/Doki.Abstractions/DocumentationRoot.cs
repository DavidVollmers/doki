namespace Doki;

public sealed record DocumentationRoot : DocumentationObject
{
    public string Name => "Packages";

    internal AssemblyDocumentation[] InternalAssemblies = [];

    public AssemblyDocumentation[] Assemblies
    {
        get => InternalAssemblies;
        init => InternalAssemblies = value;
    }

    public DocumentationRoot()
    {
        Id = "root";
        Content = DocumentationContentType.Root;
    }
}