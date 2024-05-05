namespace Doki.Output.ClassLibrary;

public sealed class ClassLibraryOutput(ClassLibraryOutputOptions options) : IOutput
{
    public Task BeginAsync(CancellationToken cancellationToken = default)
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

        File.WriteAllText(projectFilePath, projectFileContent);

        return Task.CompletedTask;
    }

    public async Task WriteAsync(ContentList contentList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);
    }
}