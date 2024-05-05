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

        foreach (var item in contentList.Items)
        {
            if (item is not AssemblyDocumentation assemblyDocumentation) continue;

            BuildAssemblyDocumentation(assemblyDocumentation, contentListContent);
        }

        contentListContent.Append("""
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
//         contentListContent.Append($"""
//                                        public static
//                                    """);
    }
}