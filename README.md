![](assets/logo-64x64.png)

[![](https://img.shields.io/github/v/release/DavidVollmers/doki?include_prereleases)](https://github.com/DavidVollmers/doki/releases)
[![](https://img.shields.io/nuget/dt/Doki.CommandLine)](https://www.nuget.org/packages/Doki.CommandLine)
[![](https://img.shields.io/github/license/DavidVollmers/doki)](https://github.com/DavidVollmers/doki/blob/master/LICENSE.txt)

# doki

> A modern .NET Documentation Framework

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
