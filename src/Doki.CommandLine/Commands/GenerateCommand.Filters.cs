using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Commands;

internal partial class GenerateCommand
{
    private const string GeneratedAssemblyName = "Doki.Generated";

    private int CompileFilters(DocumentationGenerator generator, IDictionary<string, string>? filters)
    {
        if (filters?.Any() != true)
        {
            _logger.LogDebug("No filters configured.");
            return 0;
        }

        // ReSharper disable UseRawString
        var code = $@"
using System;
using System.Reflection;

namespace Doki.Generated
{{
    public static class Filters
    {{
        {string.Join("\n", filters.Select(x => $@"
        public static readonly Func<{x.Key}, bool> {x.Key.Replace('.', '_')} = {x.Value};
"))}
    }}
}}
";
        // ReSharper restore UseRawString

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Type).Assembly.Location)
        };

        var syntax = CSharpSyntaxTree.ParseText(code);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release);
        var compilation =
            CSharpCompilation.Create(GeneratedAssemblyName, new List<SyntaxTree> { syntax }, refs, options);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var compilationErrors = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .ToList();
            foreach (var compilationError in compilationErrors)
            {
                _logger.LogError(compilationError.ToString());
            }

            return -1;
        }

        ms.Position = 0;

        var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

        foreach (var filter in filters)
        {
            var type = assembly.GetType("Doki.Generated.Filters");
            var field = type?.GetField(filter.Key.Replace('.', '_'), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
            {
                _logger.LogError("Could not find filter.");
                return -1;
            }

            var value = field.GetValue(null);
            if (value == null)
            {
                _logger.LogError("Could not get filter value.");
                return -1;
            }

            if (filter.Key == typeof(Type).FullName)
                generator.TypeFilter.Expression = (Func<Type, bool>)value;
            else if (filter.Key == typeof(ConstructorInfo).FullName)
                generator.ConstructorFilter.Expression = (Func<ConstructorInfo, bool>)value;
            else if (filter.Key == typeof(FieldInfo).FullName)
                generator.FieldFilter.Expression = (Func<FieldInfo, bool>)value;
            else if (filter.Key == typeof(PropertyInfo).FullName)
                generator.PropertyFilter.Expression = (Func<PropertyInfo, bool>)value;
            else if (filter.Key == typeof(MethodInfo).FullName)
                generator.MethodFilter.Expression = (Func<MethodInfo, bool>)value;
            else
            {
                _logger.LogError("Unsupported filter type: {FilterType}", filter.Key);
                return -1;
            }
        }

        return 0;
    }
}