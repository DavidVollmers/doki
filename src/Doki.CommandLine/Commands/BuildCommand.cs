using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Xml.XPath;
using Spectre.Console;

namespace Doki.CommandLine.Commands;

internal class BuildCommand : Command
{
    private readonly Argument<FileInfo?> _projectArgument;

    public BuildCommand() : base("build", "Builds documentation for a project.")
    {
        _projectArgument = new Argument<FileInfo?>("PROJECT", "The project to build.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        AddArgument(_projectArgument);

        this.SetHandler(HandleCommandAsync);
    }

    private async Task HandleCommandAsync(InvocationContext context)
    {
        var projectFile = context.ParseResult.GetValueForArgument(_projectArgument);

        if (projectFile == null)
        {
            var projectFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");

            switch (projectFiles.Length)
            {
                case 0:
                    AnsiConsole.MarkupLine("[bold red]No project file found.[/]");
                    return;
                case > 1:
                    AnsiConsole.MarkupLine("[bold red]Multiple project files found.[/]");
                    return;
                default:
                    projectFile = new FileInfo(projectFiles[0]);
                    break;
            }
        }

        if (new XPathDocument(projectFile.FullName).CreateNavigator()
                .SelectSingleNode("/Project/PropertyGroup/GenerateDocumentationFile")?.Value != "true")
        {
            AnsiConsole.MarkupLine("[bold red]Documentation generation is not enabled for this project.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Building project {projectFile.Name}...");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectFile.FullName}\" --no-restore --no-dependencies",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });

        if (process == null)
        {
            AnsiConsole.MarkupLine("[bold red]Could not start dotnet process.[/]");
            return;
        }

        var output = await process.StandardOutput.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            AnsiConsole.MarkupLine("[bold red]Build failed.[/]");
            AnsiConsole.MarkupLine(output);
            return;
        }
        
        AnsiConsole.MarkupLine("[bold green]Build succeeded.[/]");
        
        AnsiConsole.MarkupLine($"Generating documentation for project {projectFile.Name}...");
        
        
    }
}