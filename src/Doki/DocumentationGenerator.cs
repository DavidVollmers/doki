using System.Reflection;
using System.Xml.XPath;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

/// <summary>
/// Generates documentation for assemblies.
/// </summary>
/// <example>
/// The following example shows how to generate documentation for an assembly and output it using the <see cref="T:Doki.Output.Markdown.MarkdownOutput"/> class.
/// <code>
/// var outputContext = new OutputContext(Directory.GetCurrentDirectory());
/// 
/// var generator = new DocumentationGenerator();
///
/// generator.AddAssembly(typeof(Program).Assembly, new XPathDocument("path/to/assembly.xml"));
///
/// generator.AddOutput(new MarkdownOutput(outputContext));
///
/// await generator.GenerateAsync(new ConsoleLogger());
/// </code>
/// </example>
/// <remarks>
/// You can also use the doki cli tool to generate documentation. See the official <see href="https://github.com/DavidVollmers/doki">documentation</see> for more information.
/// </remarks>
public sealed partial class DocumentationGenerator
{
    private readonly Dictionary<Assembly, XPathNavigator> _assemblies = new();
    private readonly Dictionary<string, XPathNavigator> _projects = new();
    private readonly List<IOutput> _outputs = [];

    /// <summary>
    /// Gets the filter for types to include in the documentation.
    /// </summary>
    /// <remarks>
    /// The default filter includes only public types.
    /// </remarks>
    public Filter<Type> TypeFilter { get; } = new(t => t.IsPublic);

    /// <summary>
    /// Gets the filter for constructors to include in the documentation.
    /// </summary>
    /// <remarks>
    /// The default filter includes only public and protected constructors.
    /// </remarks>
    public Filter<ConstructorInfo> ConstructorFilter { get; } = new(c => c.IsPublic || c.IsFamily);

    /// <summary>
    /// Gets the filter for fields to include in the documentation.
    /// </summary>
    /// <remarks>
    /// The default filter includes only public and protected fields.
    /// </remarks>
    public Filter<FieldInfo> FieldFilter { get; } = new(f => !f.IsSpecialName && (f.IsPublic || f.IsFamily));

    /// <summary>
    /// Gets the filter for properties to include in the documentation.
    /// </summary>
    /// <remarks>
    /// The default filter includes only public and protected properties.
    /// </remarks>
    public Filter<PropertyInfo> PropertyFilter { get; } =
        new(p => p.GetMethod?.IsPublic == true || p.SetMethod?.IsPublic == true || p.GetMethod?.IsFamily == true ||
                 p.SetMethod?.IsFamily == true);

    /// <summary>
    /// Gets the filter for methods to include in the documentation.
    /// </summary>
    /// <remarks>
    /// The default filter includes only public and protected methods.
    /// </remarks>
    public Filter<MethodInfo> MethodFilter { get; } = new(m => !m.IsSpecialName && (m.IsPublic || m.IsFamily));

    /// <summary>
    /// Gets or sets a value indicating whether to include inherited members in the documentation.
    /// </summary>
    public bool IncludeInheritedMembers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationGenerator"/> class.
    /// </summary>
    public DocumentationGenerator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationGenerator"/> class with the specified assembly and xml documentation.
    /// </summary>
    /// <param name="assembly">The assembly to generate documentation for.</param>
    /// <param name="documentation">The xml documentation for the assembly.</param>
    public DocumentationGenerator(Assembly assembly, XPathDocument documentation)
    {
        AddAssembly(assembly, documentation);
    }

    /// <summary>
    /// Adds an assembly to generate documentation for.
    /// </summary>
    /// <param name="assembly">The assembly to generate documentation for.</param>
    /// <param name="documentation">The xml documentation for the assembly.</param>
    /// <param name="project">The optional .csproj xml from which the assembly was built.</param>
    public void AddAssembly(Assembly assembly, XPathDocument documentation, XPathDocument? project = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(documentation);

        _assemblies.Add(assembly, documentation.CreateNavigator());

        if (project != null)
        {
            _projects.Add(assembly.GetName().Name!, project.CreateNavigator());
        }
    }

    /// <summary>
    /// Adds an output to write the documentation to.
    /// </summary>
    /// <param name="output">The output to write the documentation to.</param>
    public void AddOutput(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _outputs.Add(output);
    }

    /// <summary>
    /// Generates the documentation for the assemblies and writes it to the outputs.
    /// </summary>
    /// <param name="logger">The logger to write log messages to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <exception cref="InvalidOperationException">No assemblies added for documentation.</exception>
    public async Task GenerateAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logger);

        if (_assemblies.Count == 0) throw new InvalidOperationException("No assemblies added for documentation.");

        logger.LogInformation("Generating documentation for {AssemblyCount} assemblies.", _assemblies.Count);

        var assemblies = new ContentList
        {
            Id = ContentList.Assemblies,
            Name = ContentList.Assemblies,
            Content = DocumentationContentType.Assemblies
        };

        var items = new List<DocumentationObject>();
        foreach (var (assembly, _) in _assemblies)
        {
            var assemblyDocumentation =
                await GenerateAssemblyDocumentationAsync(assembly, assemblies, logger, cancellationToken);

            if (assemblyDocumentation == null) continue;

            items.Add(assemblyDocumentation);
        }

        assemblies.Items = items.ToArray();

        foreach (var output in _outputs)
        {
            await output.WriteAsync(assemblies, cancellationToken);
        }
    }

    private async Task<AssemblyDocumentation?> GenerateAssemblyDocumentationAsync(Assembly assembly,
        DocumentationObject parent, ILogger logger, CancellationToken cancellationToken)
    {
        var assemblyName = assembly.GetName();

        var assemblyId = assemblyName.Name;
        if (assemblyId == null)
        {
            logger.LogWarning("No name found for assembly {Assembly}.", assembly);
            return null;
        }

        logger.LogInformation("Generating documentation for assembly {Assembly}.", assemblyId);

        string? packageId = null;
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        if (_projects.TryGetValue(assemblyId, out var projectMetadata))
        {
            packageId = projectMetadata.SelectSingleNode("/Project/PropertyGroup/PackageId")?.Value;

            var packageDescription = projectMetadata.SelectSingleNode("/Project/PropertyGroup/Description")?.Value;
            if (packageDescription != null) description = packageDescription;
        }

        if (description == null)
        {
            logger.LogWarning("No description found for assembly {Assembly}.", assemblyId);
        }

        var types = GetTypesToDocument(assembly).ToArray();

        var assemblyDocumentation = new AssemblyDocumentation
        {
            Id = assemblyId,
            Name = assemblyId,
            Parent = parent,
            Content = DocumentationContentType.Assembly,
            Description = description,
            FileName = assembly.Location.Split(Path.DirectorySeparatorChar).Last(),
            Version = assemblyName.Version?.ToString(),
            PackageId = packageId
        };

        logger.LogInformation("Generating documentation for {TypeCount} types.", types.Length);

        var namespaces = types.Select(t => t.Namespace!).Distinct().ToList();

        var namespaceItems = new List<DocumentationObject>();
        foreach (var @namespace in namespaces)
        {
            var namespaceDocumentation = new ContentList
            {
                Id = @namespace,
                Name = @namespace,
                Parent = assemblyDocumentation,
                Content = DocumentationContentType.Namespace
            };

            var items = new List<DocumentationObject>();
            foreach (var type in types.Where(t => t.Namespace == @namespace))
            {
                try
                {
                    var typeDocumentation =
                        await GenerateTypeDocumentationAsync(type, namespaceDocumentation, logger, cancellationToken);

                    items.Add(new TypeDocumentationReference(typeDocumentation)
                    {
                        Parent = namespaceDocumentation
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to generate documentation for type {Type}.", type);
                }
            }

            namespaceDocumentation.Items = items.ToArray();

            namespaceItems.Add(namespaceDocumentation);
        }

        assemblyDocumentation.Items = namespaceItems.ToArray();

        return assemblyDocumentation;
    }

    private async Task<TypeDocumentation> GenerateTypeDocumentationAsync(Type type, DocumentationObject parent,
        ILogger logger, CancellationToken cancellationToken)
    {
        var typeId = type.GetXmlDocumentationId();

        logger.LogDebug("Generating documentation for type {Type}.", typeId);

        var assemblyXml = _assemblies[type.Assembly];

        var typeXml = assemblyXml.SelectSingleNode($"//doc//members//member[@name='T:{typeId}']");

        var summary = typeXml?.SelectSingleNode("summary");
        if (summary == null)
        {
            logger.LogWarning("No summary found for type {Type}.", typeId);
        }

        var typeDocumentation = new TypeDocumentation
        {
            Id = typeId,
            Content = type.IsClass
                ? DocumentationContentType.Class
                : type.IsEnum
                    ? DocumentationContentType.Enum
                    : type.IsInterface
                        ? DocumentationContentType.Interface
                        : type.IsValueType
                            ? DocumentationContentType.Struct
                            : DocumentationContentType.Type,
            Parent = parent,
            Name = type.GetSanitizedName(),
            FullName = type.GetSanitizedName(true),
            Definition = type.GetTypeInfo().GetDefinition(),
            Namespace = type.Namespace,
            Assembly = type.Assembly.GetName().Name,
            IsDocumented = true,
            IsGeneric = type.IsGenericType,
        };

        if (summary != null) typeDocumentation.Summary = BuildXmlDocumentation(summary, typeDocumentation);

        typeDocumentation.GenericArguments =
            BuildGenericTypeArgumentDocumentation(type, typeDocumentation, typeXml, logger).ToArray();

        typeDocumentation.Interfaces = BuildInterfaceDocumentation(type, typeDocumentation).ToArray();

        typeDocumentation.DerivedTypes = BuildDerivedTypeDocumentation(type, typeDocumentation).ToArray();

        typeDocumentation.Examples = BuildXmlDocumentation("example", typeXml, typeDocumentation).ToArray();

        typeDocumentation.Remarks = BuildXmlDocumentation("remarks", typeXml, typeDocumentation).ToArray();

        typeDocumentation.Constructors =
            BuildConstructorDocumentation(type, typeDocumentation, assemblyXml, logger).ToArray();

        typeDocumentation.Fields = BuildFieldDocumentation(type, typeDocumentation, assemblyXml, logger).ToArray();

        typeDocumentation.Properties =
            BuildPropertyDocumentation(type, typeDocumentation, assemblyXml, logger).ToArray();

        typeDocumentation.Methods = BuildMethodDocumentation(type, typeDocumentation, assemblyXml, logger).ToArray();

        var baseType = type.BaseType;
        TypeDocumentationReference baseParent = typeDocumentation;
        while (baseType != null)
        {
            var baseTypeAssembly = baseType.Assembly.GetName();

            var typeReference = new TypeDocumentationReference
            {
                Id = baseType.GetXmlDocumentationId(),
                Content = DocumentationContentType.TypeReference,
                Parent = baseParent,
                Name = baseType.GetSanitizedName(),
                FullName = baseType.GetSanitizedName(true),
                Namespace = baseType.Namespace,
                Assembly = baseTypeAssembly.Name,
                IsDocumented = IsTypeDocumented(baseType),
                IsMicrosoft = IsAssemblyFromMicrosoft(baseTypeAssembly),
                IsGeneric = baseType.IsGenericType
            };

            typeReference.GenericArguments =
                BuildGenericTypeArgumentDocumentation(baseType, typeReference, null, logger).ToArray();

            baseParent.BaseType = typeReference;

            baseType = baseType.BaseType;
            baseParent = typeReference;
        }

        foreach (var output in _outputs)
        {
            await output.WriteAsync(typeDocumentation, cancellationToken);
        }

        return typeDocumentation;
    }

    private bool IsTypeDocumented(Type type)
    {
        return _assemblies.Any(a => a.Key.FullName == type.Assembly.FullName);
    }

    private Type? LookupType(string name)
    {
        return _assemblies.Keys.Select(assembly => assembly.GetType(name)).OfType<Type>().FirstOrDefault();
    }

    private static bool IsAssemblyFromMicrosoft(AssemblyName assemblyName)
    {
        return assemblyName.Name!.StartsWith("System") || assemblyName.Name.StartsWith("Microsoft");
    }

    private IEnumerable<Type> GetTypesToDocument(Assembly assembly)
    {
        return assembly.GetTypes().Where(TypeFilter.Expression ?? TypeFilter.Default);
    }
}