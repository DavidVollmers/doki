namespace Doki.Output.ClassLibrary;

public sealed class ClassLibraryOutput(ClassLibraryOutputOptions options) : IOutput
{
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        options.ClearOutputDirectoryIfRequired();

        options.OutputDirectory.Create();

        var projectFilePath = Path.Combine(options.OutputDirectory.FullName, $"{options.Namespace}.csproj");

        var projectFileContent = $"""
                                  <Project Sdk="Microsoft.NET.Sdk">
                                  
                                      <PropertyGroup>
                                          {options.GetTargetFrameworkProperty()}
                                          <LangVersion>12</LangVersion>
                                          <ImplicitUsings>enable</ImplicitUsings>
                                          <Nullable>enable</Nullable>
                                      </PropertyGroup>

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