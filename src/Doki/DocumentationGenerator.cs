using System.Reflection;
using System.Xml.XPath;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

/// <summary>
/// Generates documentation for assemblies.
/// </summary>
/// <example>
/// The following example shows how to generate documentation for an assembly and output it using the <see cref="T:MarkdownOutput"/> class.
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
public sealed partial class DocumentationGenerator
{
    private readonly Dictionary<Assembly, XPathNavigator> _assemblies = new();
    private readonly Dictionary<string, XPathNavigator> _projectMetadata = new();
    private readonly List<IOutput> _outputs = [];

    public DocumentationGenerator()
    {
    }

    public DocumentationGenerator(Assembly assembly, XPathDocument documentation)
    {
        AddAssembly(assembly, documentation);
    }

    public void AddAssembly(Assembly assembly, XPathDocument documentation, XPathDocument? projectMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(documentation);

        _assemblies.Add(assembly, documentation.CreateNavigator());

        if (projectMetadata != null)
        {
            _projectMetadata.Add(assembly.GetName().Name!, projectMetadata.CreateNavigator());
        }
    }

    public void AddOutput(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _outputs.Add(output);
    }

    public async Task GenerateAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logger);

        if (_assemblies.Count == 0) throw new InvalidOperationException("No assemblies added for documentation.");

        logger.LogInformation("Generating documentation for {AssemblyCount} assemblies.", _assemblies.Count);

        var assemblies = new ContentList
        {
            Id = ContentList.Assemblies,
            Name = ContentList.Assemblies,
            Content = DocumentationContent.Assemblies
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
        if (_projectMetadata.TryGetValue(assemblyId, out var projectMetadata))
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
            Content = DocumentationContent.Assembly,
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
                Content = DocumentationContent.Namespace
            };

            var items = new List<DocumentationObject>();
            foreach (var type in types.Where(t => t.Namespace == @namespace))
            {
                var typeDocumentation =
                    await GenerateTypeDocumentationAsync(type, namespaceDocumentation, logger, cancellationToken);

                items.Add(new TypeDocumentationReference(typeDocumentation)
                {
                    Parent = namespaceDocumentation
                });
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
        var typeInfo = type.GetTypeInfo();

        var typeId = typeInfo.GetSanitizedName(true, false);

        logger.LogDebug("Generating documentation for type {Type}.", typeId);

        var navigator = _assemblies[type.Assembly];

        var typeXml = navigator.SelectSingleNode($"//doc//members//member[@name='T:{typeId}']");

        var summary = typeXml?.SelectSingleNode("summary");
        if (summary == null)
        {
            logger.LogWarning("No summary found for type {Type}.", type);
        }

        var typeDocumentation = new TypeDocumentation
        {
            Id = typeId,
            Content = type.IsClass
                ? DocumentationContent.Class
                : type.IsEnum
                    ? DocumentationContent.Enum
                    : type.IsInterface
                        ? DocumentationContent.Interface
                        : type.IsValueType
                            ? DocumentationContent.Struct
                            : DocumentationContent.Type,
            Parent = parent,
            Name = typeInfo.GetSanitizedName(),
            FullName = typeInfo.GetSanitizedName(true),
            Definition = typeInfo.GetDefinition(),
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

        typeDocumentation.Examples = BuildExampleDocumentation(typeXml, typeDocumentation).ToArray();

        var baseType = typeInfo.BaseType;
        TypeDocumentationReference baseParent = typeDocumentation;
        while (baseType != null)
        {
            var baseTypeInfo = baseType.GetTypeInfo();

            var baseTypeAssembly = baseTypeInfo.Assembly.GetName();

            var typeReference = new TypeDocumentationReference
            {
                Id = baseTypeInfo.GetSanitizedName(true, false),
                Content = DocumentationContent.TypeReference,
                Parent = baseParent,
                Name = baseTypeInfo.GetSanitizedName(),
                FullName = baseTypeInfo.GetSanitizedName(true),
                Namespace = baseType.Namespace,
                Assembly = baseTypeAssembly.Name,
                IsDocumented = IsTypeDocumented(baseType),
                IsMicrosoft = IsAssemblyFromMicrosoft(baseTypeAssembly),
                IsGeneric = baseType.IsGenericType
            };

            typeReference.GenericArguments =
                BuildGenericTypeArgumentDocumentation(baseType, typeReference, null, logger).ToArray();

            baseParent.BaseType = typeReference;

            baseType = baseTypeInfo.BaseType;
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

    private static bool IsAssemblyFromMicrosoft(AssemblyName assemblyName)
    {
        return assemblyName.Name!.StartsWith("System") || assemblyName.Name.StartsWith("Microsoft");
    }

    //TODO support exclude filtering
    private static IEnumerable<Type> GetTypesToDocument(Assembly assembly)
    {
        return assembly.GetTypes().Where(a => a.IsPublic);
    }
}