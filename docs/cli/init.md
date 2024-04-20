# doki init

## Name

`doki init` - Set up your repository for documentation generation.

## Synopsis

```bash
doki init

doki init -?|-h|--help
```

## Description

The `doki init` command sets up your repository for documentation generation. It creates a `doki.config.json` file in
the root of your repository that contains the configuration for the documentation generation process.

You can customize the configuration file to suit your needs. The configuration file is used by the [doki g](generate.md)
command to generate documentation.

The `doki init` command will also update or create a `.gitignore` file in the root of your repository to exclude the
`.doki` directory from source control. The `.doki` directory is used for caching during documentation generation
process.

## Options

- `-?|-h|--help` 
  
  Show help information for the command.

## Examples

- Set up your repository for documentation generation:

  ```bash
  doki init
  ```
