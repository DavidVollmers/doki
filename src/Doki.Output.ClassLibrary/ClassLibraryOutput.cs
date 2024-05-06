﻿using System.Text;

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
            //TODO Add type documentation
        }

        content.AppendLine($$"""
                             {{i}}    ]
                             {{i}}},
                             """);
    }
}