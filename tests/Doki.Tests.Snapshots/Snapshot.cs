using System.Runtime.CompilerServices;
using Doki.Output;
using Xunit.Abstractions;

namespace Doki.Tests.Snapshots;

public class Snapshot
{
    private readonly DirectoryInfo _snapshotDirectory;

    private bool _saved;

    public string Name { get; init; }

    public DirectoryInfo OutputDirectory { get; init; } =
        new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

    private Snapshot(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        var projectPath = Path.Combine(new FileInfo(typeof(Snapshot).Assembly.Location).DirectoryName!, "..", "..",
            "..");

        _snapshotDirectory = new DirectoryInfo(Path.Combine(projectPath, "__snapshots__", Name));
    }

    public Snapshot SaveIfNotExists()
    {
        if (_snapshotDirectory.Exists) return this;

        _saved = true;

        _snapshotDirectory.Create();

        var sourcePath = OutputDirectory.FullName;
        var targetPath = _snapshotDirectory.FullName;

        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        return this;
    }

    public async Task MatchSnapshotAsync(ITestOutputHelper testOutputHelper)
    {
        if (_saved)
        {
            throw new Exception(
                "Cannot match snapshot when saving a new snapshot. Please verify the snapshot manually and/or run the test again.");
        }

        testOutputHelper.WriteLine($"Verifying snapshot '{Name}'");
        testOutputHelper.WriteLine($"Generated snapshot directory: {OutputDirectory.FullName}");

        var snapshotFiles = _snapshotDirectory.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var snapshotFile in snapshotFiles)
        {
            var relativePath = snapshotFile.FullName.Replace(_snapshotDirectory.FullName, "");
            var sourceFile = new FileInfo(OutputDirectory.FullName + relativePath);

            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException($"Missing snapshot file: {relativePath}");
            }

            var snapshotContent = await File.ReadAllTextAsync(snapshotFile.FullName);
            var sourceContent = await File.ReadAllTextAsync(sourceFile.FullName);

            if (snapshotContent != sourceContent)
            {
                testOutputHelper.WriteLine($"Snapshot file '{snapshotFile.FullName}' does not match.");
                testOutputHelper.WriteLine("Expected:");
                testOutputHelper.WriteLine(snapshotContent);
                testOutputHelper.WriteLine("Actual:");
                testOutputHelper.WriteLine(sourceContent);

                throw new Exception(
                    $"Snapshot file '{snapshotFile.FullName}' does not match. See test output for details.");
            }
        }
    }

    public static Snapshot Create([CallerMemberName] string? name = null)
    {
        return new Snapshot(name!);
    }
}