# Contributing

Thanks for your interest in contributing! Please take a moment to review this document in order to make the contribution
process easy and effective for everyone involved.

## Pull requests

**Please ask first before starting work on any significant changes/features!**

It is never a fun experience to put a lot of work into a pull request only to have it rejected because it doesn't fit
with the project's goals or because someone else is already working on it.

## Testing doki locally

To test doki locally, you first need to build the NuGet package:

```bash
dotnet pack -c Release -p:Version=0.2.0-preview
```

Then you can install the package as a local dotnet tool:

```bash
dotnet tool install --global --add-source .\nuget --version 0.2.0-preview Doki.CommandLine
```

> If you already have the tool installed, you can update it with the following command:
> ```bash
> dotnet tool update --global --add-source .\nuget --version 0.2.0-preview Doki.CommandLine
> ```

Now you can use your version of the doki CLI on your local machine.
