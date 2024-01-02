using System.Reflection;

namespace Doki;

internal static class ParameterInfoExtensions
{
    public static string? GetParametersXmlDocumentationIdString(IEnumerable<ParameterInfo> parameters,
        string[]? genericClassParams = null)
    {
        var parameterStrings = parameters
            .Select(parameterInfo => parameterInfo.ParameterType.GetXmlDocumentationIdCore(
                parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef,
                true,
                genericClassParams))
            .ToArray();
        return parameterStrings.Length > 0 ? $"({string.Join(",", parameterStrings)})" : null;
    }
}