namespace Doki.Output;

// ReSharper disable once UnusedTypeParameter
public interface IOutputOptions<T> where T : IOutput
{
    DirectoryInfo OutputDirectory { get; }
}