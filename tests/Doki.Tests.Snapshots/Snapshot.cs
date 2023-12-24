using System.Runtime.CompilerServices;
using Doki.Output;
using Xunit.Abstractions;

namespace Doki.Tests.Snapshots;

public class Snapshot
{
    private bool _created;

    public string Name { get; init; }

    public OutputContext Context { get; init; } =
        new(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))), null);

    private Snapshot(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Snapshot CreateIfNotExists()
    {
        var projectPath = Path.Combine(new FileInfo(typeof(Snapshot).Assembly.Location).DirectoryName!, "..", "..",
            "..");

        var snapshotDirectory = new DirectoryInfo(Path.Combine(projectPath, "__snapshots__", Name));

        if (snapshotDirectory.Exists) return this;

        _created = true;

        snapshotDirectory.Create();

        var sourcePath = Context.WorkingDirectory.FullName;
        var targetPath = snapshotDirectory.FullName;

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

        Assert.False(true);
    }

    public static Snapshot Create([CallerMemberName] string? name = null)
    {
        return new Snapshot(name!);
    }
}