using Doki.CommandLine.NuGet;
using Doki.Tests.Common;
using Xunit.Abstractions;

namespace Doki.CommandLine.Tests;

public class NuGetTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Test_NuGetLoader()
    {
        const string packageId = "Doki.Output.Json";
        var tmpDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var logger = new TestOutputLogger(testOutputHelper);

        using var loader = new NuGetLoader(logger);

        await loader.LoadPackageAsync(packageId, tmpDirectory);

        Assert.False(logger.HadError);

        var dllPath = Path.Combine(tmpDirectory, $"{packageId}.1.0.0", "lib", "net8.0", $"{packageId}.dll");

        Assert.True(File.Exists(dllPath));
    }
}