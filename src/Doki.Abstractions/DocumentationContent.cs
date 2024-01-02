namespace Doki;

/// <summary>
/// The content of a documentation object.
/// </summary>
public enum DocumentationContent
{
    /// <summary>
    /// The root of the documentation, containing all assemblies/packages.
    /// </summary>
    Assemblies,
    Assembly,
    Namespace,
    TypeReference,
    Class,
    Enum,
    Struct,
    Interface,
    Type,
    GenericTypeArgument,
    XmlDocumentation,
    Text,
    CodeBlock,
    Link,
    Constructor,
    Field,
    Property
}