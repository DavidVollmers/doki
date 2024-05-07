using System.Text;

namespace Doki.Output.ClassLibrary;

public sealed class ClassLibraryOutput(ClassLibraryOutputOptions options) : IOutput
{
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        options.ClearOutputDirectoryIfRequired();

        options.OutputDirectory.Create();

        var projectFilePath = Path.Combine(options.OutputDirectory.FullName, $"{options.Namespace}.csproj");

#if DEBUG
        const string dokiReference = "<ProjectReference Include=\"..\\Doki.Abstractions\\Doki.Abstractions.csproj\" />";
#else
        var dokiVersion = typeof(DocumentationObject).Assembly.GetName().Version!.ToString();
        var dokiReference = $"<PackageReference Include=\"Doki.Abstractions\" Version=\"{dokiVersion}\" />";
#endif

        var projectFileContent = $"""
                                  <Project Sdk="Microsoft.NET.Sdk">
                                  
                                      <PropertyGroup>
                                          {options.GetTargetFrameworkProperty()}
                                          <LangVersion>12</LangVersion>
                                          <ImplicitUsings>enable</ImplicitUsings>
                                          <Nullable>enable</Nullable>
                                      </PropertyGroup>
                                  
                                      <ItemGroup>
                                          {dokiReference}
                                      </ItemGroup>

                                  </Project>
                                  """;

        await File.WriteAllTextAsync(projectFilePath, projectFileContent, cancellationToken);
    }

    public async Task WriteAsync(DocumentationRoot root, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(root);

        var filePath = Path.Combine(options.OutputDirectory.FullName, "Documentation.cs");

        var content = new StringBuilder($$"""
                                          using Doki;
                                           
                                          namespace {{options.Namespace}};

                                          public static class Documentation
                                          {
                                              public static readonly AssemblyDocumentation[] Assemblies =
                                              [
                                          """);
        content.AppendLine();

        foreach (var assemblyDocumentation in root.Assemblies)
        {
            BuildAssemblyDocumentation(assemblyDocumentation, content);
        }

        content.AppendLine("""
                               ];
                           }
                           """);

        await File.WriteAllTextAsync(filePath, content.ToString(), cancellationToken);
    }

    private static void BuildAssemblyDocumentation(AssemblyDocumentation assemblyDocumentation, StringBuilder content)
    {
        content.AppendLine($$"""
                                     new AssemblyDocumentation
                                     {
                                         Name = "{{assemblyDocumentation.Name}}",
                                         Description = "{{assemblyDocumentation.Description}}",
                                         FileName = "{{assemblyDocumentation.FileName}}",
                                         Version = "{{assemblyDocumentation.Version}}",
                                         PackageId = "{{assemblyDocumentation.PackageId}}",
                                         Namespaces =
                                         [
                             """);

        foreach (var namespaceDocumentation in assemblyDocumentation.Namespaces)
        {
            BuildNamespaceDocumentation(namespaceDocumentation, content, 4);
        }

        content.AppendLine("""
                                       ]
                                   },
                           """);
    }

    private static void BuildNamespaceDocumentation(NamespaceDocumentation namespaceDocumentation,
        StringBuilder content, int indent)
    {
        var i = new string(' ', indent * 4);

        content.AppendLine($$"""
                             {{i}}new NamespaceDocumentation
                             {{i}}{
                             {{i}}    Name = "{{namespaceDocumentation.Name}}",
                             {{i}}    Description = "{{namespaceDocumentation.Description}}",
                             {{i}}    Types =
                             {{i}}    [
                             """);

        foreach (var typeDocumentation in namespaceDocumentation.Types)
        {
            BuildTypeDocumentation(typeDocumentation, content, indent + 2);
        }

        content.AppendLine($$"""
                             {{i}}    ]
                             {{i}}},
                             """);
    }

    private static void BuildTypeDocumentation(TypeDocumentation typeDocumentation, StringBuilder content, int indent)
    {
        var i = new string(' ', indent * 4);

        content.AppendLine($$"""
                             {{i}}new TypeDocumentation
                             {{i}}{
                             {{i}}    ContentType = DocumentationContentType.{{Enum.GetName(typeDocumentation.ContentType)}},
                             {{i}}    Definition = "{{typeDocumentation.Definition}}",
                             {{i}}    Examples =
                             {{i}}    [
                             """);

        foreach (var xmlDocumentation in typeDocumentation.Examples)
        {
            BuildXmlDocumentation(xmlDocumentation, content, indent + 2);
        }

        content.AppendLine($"""
                            {i}    ],
                            {i}    Remarks =
                            {i}    [
                            """);

        foreach (var xmlDocumentation in typeDocumentation.Remarks)
        {
            BuildXmlDocumentation(xmlDocumentation, content, indent + 2);
        }

        content.AppendLine($"""
                            {i}    ],
                            {i}    Interfaces =
                            {i}    [
                            """);

        //TODO Interfaces

        content.AppendLine($"""
                            {i}    ],
                            {i}    DerivedTypes =
                            {i}    [
                            """);

        //TODO DerivedTypes

        content.AppendLine($"""
                            {i}    ],
                            {i}    Constructors =
                            {i}    [
                            """);

        //TODO Constructors

        content.AppendLine($"""
                            {i}    ],
                            {i}    Fields =
                            {i}    [
                            """);

        //TODO Fields

        content.AppendLine($"""
                            {i}    ],
                            {i}    Properties =
                            {i}    [
                            """);

        //TODO Properties

        content.AppendLine($"""
                            {i}    ],
                            {i}    Methods =
                            {i}    [
                            """);

        //TODO Methods

        content.AppendLine($"{i}    ],");

        BuildTypeDocumentationReferenceProperties(typeDocumentation, content, indent);

        content.AppendLine($$"""
                             {{i}}},
                             """);
    }

    private static void BuildTypeDocumentationReferenceProperties(TypeDocumentationReference typeDocumentationReference,
        StringBuilder content, int indent)
    {
        var i = new string(' ', indent * 4);

        content.Append($"""
                        {i}    IsGeneric = {typeDocumentationReference.IsGeneric.ToString().ToLowerInvariant()},
                        {i}    FullName = "{typeDocumentationReference.FullName}",
                        {i}    IsDocumented = {typeDocumentationReference.IsDocumented.ToString().ToLowerInvariant()},
                        {i}    IsMicrosoft = {typeDocumentationReference.IsMicrosoft.ToString().ToLowerInvariant()},
                        {i}    BaseType =
                        """);

        if (typeDocumentationReference.BaseType == null) content.AppendLine(" null,");
        //TODO BuildTypeDocumentationReference(typeDocumentation.BaseType, content, indent + 1);
        else content.AppendLine(" null,");

        content.AppendLine($"""
                            {i}    GenericArguments =
                            {i}    [
                            """);

        foreach (var genericTypeArgumentDocumentation in typeDocumentationReference.GenericArguments)
        {
            //TODO BuildGenericTypeArgumentDocumentation(genericTypeArgumentDocumentation, content, indent + 1);
        }

        content.AppendLine($"{i}    ],");

        BuildMemberDocumentationProperties(typeDocumentationReference, content, indent);
    }

    private static void BuildMemberDocumentationProperties(MemberDocumentation memberDocumentation,
        StringBuilder content, int indent)
    {
        var i = new string(' ', indent * 4);

        content.Append($"""
                        {i}    Name = "{memberDocumentation.Name}",
                        {i}    Namespace = "{memberDocumentation.Namespace}",
                        {i}    Assembly = "{memberDocumentation.Assembly}",
                        {i}    Summary =
                        """);

        if (memberDocumentation.Summary == null) content.AppendLine(" null,");
        else BuildXmlDocumentation(memberDocumentation.Summary, content, indent + 1, true);
    }

    private static void BuildXmlDocumentation(XmlDocumentation xmlDocumentation, StringBuilder content, int indent,
        bool startInline = false)
    {
        var i = new string(' ', indent * 4);

        content.Append(startInline ? " " : i);

        content.AppendLine($$"""
                             new XmlDocumentation
                             {{i}}{
                             {{i}}    Name = "{{xmlDocumentation.Name}}",
                             {{i}}    Contents =
                             {{i}}    [
                             """);

        foreach (var documentationObject in xmlDocumentation.Contents)
        {
            //TODO BuildDocumentationObject(documentationObject, content, indent + 1);
        }

        content.AppendLine($$"""
                             {{i}}    ]
                             {{i}}},
                             """);
    }
}