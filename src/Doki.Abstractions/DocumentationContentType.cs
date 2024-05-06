namespace Doki;

/// <summary>
/// The content type of documentation object.
/// </summary>
public enum DocumentationContentType
{
    /// <summary>
    /// The root of the documentation, containing all assemblies/packages.
    /// </summary>
    Root,
    /// <summary>
    /// An assembly in the documentation.
    /// </summary>
    Assembly,
    /// <summary>
    /// A namespace in the documentation.
    /// </summary>
    Namespace,
    /// <summary>
    /// A type reference in the documentation.
    /// </summary>
    TypeReference,
    /// <summary>
    /// A class in the documentation.
    /// </summary>
    Class,
    /// <summary>
    /// An enum in the documentation.
    /// </summary>
    Enum,
    /// <summary>
    /// A struct in the documentation.
    /// </summary>
    Struct,
    /// <summary>
    /// An interface in the documentation.
    /// </summary>
    Interface,
    /// <summary>
    /// A type.
    /// </summary>
    Type,
    /// <summary>
    /// A generic type argument.
    /// </summary>
    GenericTypeArgument,
    /// <summary>
    /// A xml documentation object.
    /// </summary>
    XmlDocumentation,
    /// <summary>
    /// A text content.
    /// </summary>
    Text,
    /// <summary>
    /// A code block.
    /// </summary>
    CodeBlock,
    /// <summary>
    /// A link.
    /// </summary>
    Link,
    /// <summary>
    /// A constructor in the documentation.
    /// </summary>
    Constructor,
    /// <summary>
    /// A field in the documentation.
    /// </summary>
    Field,
    /// <summary>
    /// A property in the documentation.
    /// </summary>
    Property,
}