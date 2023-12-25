using System.Reflection;
using System.Runtime.CompilerServices;
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
            Content = DokiContent.Assemblies
        };

        var items = new List<DokiElement>();
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

    private async Task<AssemblyDocumentation?> GenerateAssemblyDocumentationAsync(Assembly assembly, DokiElement parent,
        ILogger logger, CancellationToken cancellationToken)
    {
        var assemblyName = assembly.GetName();

        var assemblyId = assemblyName.Name;
        if (assemblyId == null)
        {
            logger.LogWarning("No name found for assembly {Assembly}.", assembly);
            return null;
        }

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

        logger.LogInformation("Generating documentation for {TypeCount} types.", types.Length);

        var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

        var namespaceItems = new List<DokiElement>();
        foreach (var @namespace in namespaces)
        {
            var namespaceDocumentation = new ContentList
            {
                Id = @namespace!,
                Parent = parent,
                Content = DokiContent.Namespace
            };

            var items = new List<DokiElement>();
            foreach (var type in types.Where(t => t.Namespace == @namespace))
            {
                var typeDocumentation =
                    await GenerateTypeDocumentationAsync(type, namespaceDocumentation, logger, cancellationToken);

                items.Add(new TypeDocumentationReference
                {
                    Id = typeDocumentation.Id,
                    Parent = namespaceDocumentation,
                    Content = DokiContent.TypeReference,
                    Properties = typeDocumentation.Properties
                });
            }

            namespaceDocumentation.Items = items.ToArray();

            namespaceItems.Add(namespaceDocumentation);
        }

        return new AssemblyDocumentation
        {
            Id = assemblyId,
            Parent = parent,
            Content = DokiContent.Assembly,
            Items = namespaceItems.ToArray(),
            Properties = new Dictionary<string, object?>
            {
                { DokiProperties.Description, description },
                { DokiProperties.FileName, assembly.Location.Split(Path.DirectorySeparatorChar).Last() },
                { DokiProperties.Name, assemblyName.Name },
                { DokiProperties.Version, assemblyName.Version?.ToString() },
                { DokiProperties.PackageId, packageId }
            }
        };
    }

    private async Task<TypeDocumentation> GenerateTypeDocumentationAsync(Type type, DokiElement parent, ILogger logger,
        CancellationToken cancellationToken)
    {
        var typeInfo = type.GetTypeInfo();

        var navigator = _assemblies[type.Assembly];

        var typeXml = navigator.SelectSingleNode($"//doc//members//member[@name='T:{type}']");

        var summary = typeXml?.SelectSingleNode("summary")?.Value;
        if (summary == null)
        {
            logger.LogWarning("No summary found for type {Type}.", type);
        }

        var typeDocumentation = new TypeDocumentation
        {
            Id = typeInfo.GetSanitizedName(true, false),
            Content = type.IsClass
                ? DokiContent.Class
                : type.IsEnum
                    ? DokiContent.Enum
                    : type.IsInterface
                        ? DokiContent.Interface
                        : type.IsValueType
                            ? DokiContent.Struct
                            : DokiContent.Type,
            Parent = parent,
            Properties = new Dictionary<string, object?>
            {
                { DokiProperties.Name, typeInfo.GetSanitizedName() },
                { DokiProperties.FullName, typeInfo.GetSanitizedName(true) },
                { DokiProperties.Summary, summary?.Trim() },
                { DokiProperties.Definition, typeInfo.GetDefinition() },
                { DokiProperties.IsDocumented, true },
                { DokiProperties.IsMicrosoft, false }
            }
        };

        var baseType = typeInfo.BaseType;
        DokiElement baseParent = typeDocumentation;
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
                Content = DokiContent.TypeReference,
                Parent = baseParent,
                Properties = new Dictionary<string, object?>
                {
                    { DokiProperties.Name, baseTypeInfo.GetSanitizedName() },
                    { DokiProperties.FullName, baseTypeInfo.GetSanitizedName(true) },
                    { DokiProperties.Definition, baseTypeInfo.GetDefinition() },
                    { DokiProperties.IsDocumented, isDocumented },
                    { DokiProperties.IsMicrosoft, isMicrosoft }
                }
            };

            ((Dictionary<string, object?>)baseParent.Properties).Add("BaseType", typeReference);

            baseType = baseTypeInfo.BaseType;
            baseParent = typeReference;
        }

        foreach (var output in _outputs)
        {
            await output.WriteAsync(typeDocumentation, cancellationToken);
        }

        return typeDocumentation;
    }

    //TODO support exclude filtering
    private static IEnumerable<Type> GetTypesToDocument(Assembly assembly)
    {
        return assembly.GetTypes().Where(a => a.IsPublic);
    }
}