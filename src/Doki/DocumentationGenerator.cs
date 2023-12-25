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

        var assembliesToC = new TableOfContents
        {
            Id = TableOfContents.Assemblies,
            Content = DokiContent.Assemblies
        };

        var children = new List<DokiElement>();
        foreach (var (assembly, _) in _assemblies)
        {
            var assemblyName = assembly.GetName();

            var assemblyId = assemblyName.Name;
            if (assemblyId == null)
            {
                logger.LogWarning("No name found for assembly {Assembly}.", assembly);
                continue;
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

            var assemblyToC = new TableOfContents
            {
                Id = assemblyId,
                Parent = assembliesToC,
                Content = DokiContent.Assembly,
                Properties = new Dictionary<string, object?>
                {
                    {DokiProperties.Description, description},
                    {DokiProperties.FileName, assembly.Location.Split(Path.DirectorySeparatorChar).Last()},
                    {DokiProperties.Name, assemblyName.Name},
                    {DokiProperties.Version, assemblyName.Version?.ToString()},
                    {DokiProperties.PackageId, packageId}
                }
            };

            var namespaceToCs = GenerateAssemblyDocumentationAsync(assemblyToC, assembly, logger, cancellationToken);

            assemblyToC.Children = await namespaceToCs.ToArrayAsync(cancellationToken);

            children.Add(assemblyToC);
        }

        assembliesToC.Children = children.ToArray();

        foreach (var output in _outputs)
        {
            await output.WriteAsync(assembliesToC, cancellationToken);
        }
    }

    private async IAsyncEnumerable<TableOfContents> GenerateAssemblyDocumentationAsync(TableOfContents parent,
        Assembly assembly, ILogger logger, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var types = GetTypesToDocument(assembly).ToArray();

        logger.LogInformation("Generating documentation for {TypeCount} types.", types.Length);

        var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

        foreach (var @namespace in namespaces)
        {
            var namespaceToC = new TableOfContents
            {
                Id = @namespace!,
                Parent = parent,
                Content = DokiContent.Namespace
            };

            var children = new List<DokiElement>();
            foreach (var type in types.Where(t => t.Namespace == @namespace))
            {
                var typeDocumentation =
                    await GenerateTypeDocumentationAsync(type, namespaceToC, logger, cancellationToken);

                children.Add(new TypeDocumentationReference
                {
                    Id = typeDocumentation.Id,
                    Parent = namespaceToC,
                    Content = DokiContent.TypeReference,
                    Properties = typeDocumentation.Properties
                });
            }

            namespaceToC.Children = children.ToArray();

            yield return namespaceToC;
        }
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
                {DokiProperties.Name, typeInfo.GetSanitizedName()},
                {DokiProperties.FullName, typeInfo.GetSanitizedName(true)},
                {DokiProperties.Summary, summary?.Trim()},
                {DokiProperties.Definition, typeInfo.GetDefinition()},
                {DokiProperties.IsDocumented, true},
                {DokiProperties.IsMicrosoft, false}
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
                    {DokiProperties.Name, baseTypeInfo.GetSanitizedName()},
                    {DokiProperties.FullName, baseTypeInfo.GetSanitizedName(true)},
                    {DokiProperties.Definition, baseTypeInfo.GetDefinition()},
                    {DokiProperties.IsDocumented, isDocumented},
                    {DokiProperties.IsMicrosoft, isMicrosoft}
                }
            };

            ((Dictionary<string, object?>) baseParent.Properties).Add("BaseType", typeReference);

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