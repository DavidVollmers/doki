[![](https://img.shields.io/github/v/release/DavidVollmers/doki?include_prereleases)](https://github.com/DavidVollmers/doki/releases)
[![](https://img.shields.io/nuget/dt/Doki.CommandLine)](https://www.nuget.org/packages/Doki.CommandLine)
[![](https://img.shields.io/github/license/DavidVollmers/doki)](https://github.com/DavidVollmers/doki/blob/master/LICENSE.txt)

![](assets/logo-64x64.png)

# doki

Doki is a .NET code documentation framework that allows you to generate documentation from your codebase.

It uses the XML documentation comments in your code to generate documentation in various formats, such as Markdown,
Blazor, and more.

> **Note:** Doki is still in preview. Currently, only the Markdown output is available.

## Getting Started

Install the `doki` Command-Line Interface:

```bash
dotnet tool install --global Doki.CommandLine --version 0.3.0-preview
```

> **Note:** You can find the latest version of the CLI and how to install it
> on [NuGet](https://www.nuget.org/packages/Doki.CommandLine).

Set up your repository:

```bash
doki init
```

Generate documentation:

```bash
doki g --allow-preview
```

> **Note:** Doki is still in preview. Use the `--allow-preview` option to allow preview versions of the configured
> output libraries to be used during documentation generation.

## Documentation

Doki is documented using doki. You can find the generated API documentation [here](docs/api/README.md).

You can find the CLI documentation [here](docs/cli/README.md).

---

## License

This project is licensed under the [MIT](LICENSE.txt) license.
