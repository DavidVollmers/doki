using System.Reflection;
using System.Runtime.Loader;

namespace Doki.CommandLine;

internal class DokiAssemblyLoadContext(string assemblyPath) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(assemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // ReSharper disable once LocalVariableHidesPrimaryConstructorParameter
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}