# doki g

## Name

`doki g` - Generate documentation for your .NET projects.

## Synopsis

```bash
doki g [<CONFIG>] [--allow-preview] [-c|--configuration <CONFIGURATION>] [--no-build]

doki g -?|-h|--help
```

## Description

The `doki g` command generates documentation for your .NET projects using the Doki Documentation Framework.

The command will build your project/s and generate documentation based on the configuration in the
`doki.config.json` file.

To set up a configuration file, use the [doki init](init.md) command.

## Arguments

`CONFIG`

The path to the configuration file to use for documentation generation. If not specified, the command will look for a
`doki.config.json` file in the current directory.

## Options

- `--allow-preview`

  Allow preview versions of the configured output libraries to be used during documentation generation.


- `-c|--configuration <CONFIGURATION>`

  Defines the build configuration for building projects. If not specified, the command will use the `Release`
  configuration.


- `--no-build`

  Skip building the project/s before generating documentation.
  > **Note:** If you use this option, you must ensure that the project/s are built before running the command and that
  > the output directories include all project dependencies and their XML documentation files.

## Examples

- Generate documentation:

  ```bash
  doki g
  ```

- Generate documentation using a specific configuration file:

  ```bash
  doki g path/to/doki.config.json
  ```

- Generate documentation using a specific build configuration:

  ```bash
  doki g -c Debug
  ```

- Generate documentation without building the project/s:

  ```bash
  doki g --no-build
  ```
