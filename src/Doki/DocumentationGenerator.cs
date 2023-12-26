using System.Reflection;
using System.Xml.XPath;
using Doki.Output;
using Microsoft.Extensions.Logging;

namespace Doki;

/// <summary>
/// Generates documentation for assemblies.
/// </summary>
public sealed class DocumentationGenerator
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
        DocumentationObject parent,
        ILogger logger, CancellationToken cancellationToken)
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
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var typeInfo = type.GetTypeInfo();

        var typeId = typeInfo.GetSanitizedName(true, false);

        logger.LogDebug("Generating documentation for type {Type}.", typeId);

        var navigator = _assemblies[type.Assembly];
        
        var typeXml = navigator.SelectSingleNode($"//doc//members//member[@name='T:{typeId}']");

        var summary = typeXml?.SelectSingleNode("summary")?.Value;
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
            Summary = summary?.Trim(),
            Definition = typeInfo.GetDefinition(),
            Namespace = type.Namespace,
            Assembly = type.Assembly.GetName().Name,
            IsDocumented = true,
            IsGeneric = type.IsGenericType
        };

        typeDocumentation.GenericArguments =
            BuildGenericTypeArgumentDocumentation(type, typeDocumentation, typeXml, logger);

        var baseType = typeInfo.BaseType;
        TypeDocumentationReference baseParent = typeDocumentation;
        while (baseType != null)
        {
            var baseTypeInfo = baseType.GetTypeInfo();

            var isDocumented = _assemblies.Any(a => a.Key.FullName == baseTypeInfo.Assembly.FullName);

            var baseTypeAssembly = baseTypeInfo.Assembly.GetName();
            var isMicrosoft = baseTypeAssembly.Name!.StartsWith("System") ||
                              baseTypeAssembly.Name.StartsWith("Microsoft");

            var typeReference = new TypeDocumentationReference
            {
                Id = baseTypeInfo.GetSanitizedName(true, false),
                Content = DocumentationContent.TypeReference,
                Parent = baseParent,
                Name = baseTypeInfo.GetSanitizedName(),
                FullName = baseTypeInfo.GetSanitizedName(true),
                Definition = baseTypeInfo.GetDefinition(),
                Namespace = baseType.Namespace,
                Assembly = baseTypeAssembly.Name,
                IsDocumented = isDocumented,
                IsMicrosoft = isMicrosoft,
                IsGeneric = baseType.IsGenericType
            };

            typeReference.GenericArguments =
                BuildGenericTypeArgumentDocumentation(type, typeReference, typeXml, logger);

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

    private static IEnumerable<GenericTypeArgumentDocumentation> BuildGenericTypeArgumentDocumentation(Type type,
        DocumentationObject parent, XPathNavigator? typeXml, ILogger logger)
    {
        if (!type.IsGenericType) yield break;

        var genericArguments = type.GetGenericArguments();

        foreach (var genericArgument in genericArguments)
        {
            var genericArgumentInfo = genericArgument.GetTypeInfo();

            var genericArgumentId = genericArgument.IsGenericParameter
                ? genericArgument.Name
                : genericArgumentInfo.GetSanitizedName(true, false);

            logger.LogDebug("Generating documentation for generic argument {GenericArgument}.", genericArgumentId);

            var description = typeXml?.SelectSingleNode($"typeparam[@name='{genericArgumentInfo.Name}']")?.Value;
            if (description == null)
            {
                logger.LogWarning("No description found for generic argument {GenericArgument}.", genericArgument);
            }

            yield return new GenericTypeArgumentDocumentation
            {
                Id = genericArgumentId,
                Name = genericArgument.Name,
                Content = DocumentationContent.GenericTypeArgument,
                Description = description?.Trim(),
                Namespace = genericArgument.Namespace,
                Assembly = genericArgument.Assembly.GetName().Name,
                IsGeneric = genericArgument.IsGenericType,
                Parent = parent
            };
        }
    }

    //TODO support exclude filtering
    private static IEnumerable<Type> GetTypesToDocument(Assembly assembly)
    {
        return assembly.GetTypes().Where(a => a.IsPublic);
    }
}