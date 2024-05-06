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

    public async Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);

        var contentListFilePath = Path.Combine(options.OutputDirectory.FullName, "Documentation.cs");

        var contentListContent = new StringBuilder($$"""
                                                     using Doki;
                                                      
                                                     namespace {{options.Namespace}};

                                                     public static class Documentation
                                                     {
                                                         public static readonly AssemblyDocumentation[] Assemblies =
                                                         [
                                                     """);
        contentListContent.AppendLine();

        foreach (var item in contentList.Items)
        {
            if (item is not AssemblyDocumentation assemblyDocumentation) continue;

            BuildAssemblyDocumentation(assemblyDocumentation, contentListContent);
        }

        contentListContent.AppendLine("""
                                          ];
                                      }
                                      """);

        await File.WriteAllTextAsync(contentListFilePath, contentListContent.ToString(), cancellationToken);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);
    }

    private static void BuildAssemblyDocumentation(AssemblyDocumentation assemblyDocumentation,
        StringBuilder contentListContent)
    {
        contentListContent.AppendLine($$"""
                                                new AssemblyDocumentation
                                                {
                                                    Name = "{{assemblyDocumentation.Name}}",
                                                    Description = "{{assemblyDocumentation.Description}}",
                                                    FileName = "{{assemblyDocumentation.FileName}}",
                                                    Version = "{{assemblyDocumentation.Version}}",
                                                    PackageId = "{{assemblyDocumentation.PackageId}}",
                                                    Items =
                                                    [
                                        """);

        foreach (var item in assemblyDocumentation.Items)
        {
            BuildContentList(item, contentListContent, 4);
        }

        contentListContent.AppendLine("""
                                                  ]
                                              },
                                      """);
    }

    private static void BuildContentList(DocumentationObject documentationObject, StringBuilder contentListContent,
        int indent)
    {
        if (documentationObject is not ContentList contentList) return;

        var i = new string(' ', indent * 4);

        contentListContent.AppendLine($$"""
                                        {{i}}new ContentList
                                        {{i}}{
                                        {{i}}    Name = "{{contentList.Name}}",
                                        {{i}}    Description = "{{contentList.Description}}",
                                        {{i}}    Items =
                                        {{i}}    [
                                        """);

        foreach (var item in contentList.Items)
        {
            if (item is TypeDocumentationReference typeDocumentationReference)
            {
                //TODO - Add type documentation reference
                continue;
            }

            BuildContentList(item, contentListContent, indent + 2);
        }

        contentListContent.AppendLine($$"""
                                        {{i}}    ]
                                        {{i}}},
                                        """);
    }
}