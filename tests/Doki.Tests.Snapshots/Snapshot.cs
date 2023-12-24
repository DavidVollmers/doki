using System.Runtime.CompilerServices;
using Doki.Output;
using Xunit.Abstractions;

namespace Doki.Tests.Snapshots;

public class Snapshot
{
    private bool _created;
    private DirectoryInfo _snapshotDirectory;

    public string Name { get; init; }

    public OutputContext Context { get; init; } =
        new(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))), null);

    private Snapshot(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        var projectPath = Path.Combine(new FileInfo(typeof(Snapshot).Assembly.Location).DirectoryName!, "..", "..",
            "..");

        _snapshotDirectory = new DirectoryInfo(Path.Combine(projectPath, "__snapshots__", Name));
    }

    public Snapshot CreateIfNotExists()
    {
        if (_snapshotDirectory.Exists) return this;

        _created = true;

        _snapshotDirectory.Create();

        var sourcePath = Context.WorkingDirectory.FullName;
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
        if (_created)
        {
            throw new Exception(
                "Cannot match snapshot when creating a new snapshot. Please verify the snapshot manually and/or run the test again.");
        }

        var snapshotFiles = _snapshotDirectory.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var snapshotFile in snapshotFiles)
        {
            var relativePath = snapshotFile.FullName.Replace(_snapshotDirectory.FullName, "");
            var sourceFile = new FileInfo(Context.WorkingDirectory.FullName + relativePath);

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