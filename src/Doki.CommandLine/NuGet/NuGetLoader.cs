using Microsoft.Extensions.DependencyModel;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Doki.CommandLine.NuGet;

// inspired by https://gist.github.com/alistairjevans/4de1dccfb7288e0460b7b04f9a700a04
internal class NuGetLoader : IDisposable
{
    private readonly SourceCacheContext _cacheContext = new();

    //TODO use console logger (ASCII)
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly SourceRepositoryProvider _sourceRepositoryProvider;

    public NuGetLoader(string? source = null)
    {
        var sources = new List<PackageSource>
        {
            new("https://api.nuget.org/v3/index.json")
        };

        if (source != null) sources.Add(new PackageSource(source));

        var sourceProvider = new PackageSourceProvider(NullSettings.Instance, sources);

        _sourceRepositoryProvider = new SourceRepositoryProvider(sourceProvider, Repository.Provider.GetCoreV3());
    }

    public async Task<string> LoadPackageAsync(string packageId, string destinationDirectory, bool allowPreview = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageId, nameof(packageId));

        var packageIdentity = await GetPackageIdentityAsync(packageId, allowPreview, cancellationToken);

        var dependencyInfos = new HashSet<SourcePackageDependencyInfo>();
        await CollectPackagesAsync(packageIdentity, dependencyInfos, cancellationToken);

        var packagesToInstall = GetPackagesToInstall(packageId, dependencyInfos.ToArray());

        var settings = Settings.LoadDefaultSettings(destinationDirectory);

        await InstallPackagesAsync(packagesToInstall, destinationDirectory, settings, cancellationToken);

        return Path.Combine(destinationDirectory, $"{packageIdentity.Id}.{packageIdentity.Version}", "lib", "net8.0",
            $"{packageIdentity.Id}.dll");
    }

    private async Task InstallPackagesAsync(IEnumerable<SourcePackageDependencyInfo> packagesToInstall,
        string destinationDirectory, ISettings settings, CancellationToken cancellationToken)
    {
        var packagePathResolver = new PackagePathResolver(destinationDirectory);
        var packageExtractionContext = new PackageExtractionContext(
            PackageSaveMode.Defaultv3,
            XmlDocFileSaveMode.Skip,
            ClientPolicyContext.GetClientPolicy(settings, _logger),
            _logger);

        foreach (var package in packagesToInstall)
        {
            var downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);

            var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                package,
                new PackageDownloadContext(_cacheContext),
                SettingsUtility.GetGlobalPackagesFolder(settings),
                _logger,
                cancellationToken);

            await PackageExtractor.ExtractPackageAsync(
                downloadResult.PackageSource,
                downloadResult.PackageStream,
                packagePathResolver,
                packageExtractionContext,
                cancellationToken);
        }
    }

    private IEnumerable<SourcePackageDependencyInfo> GetPackagesToInstall(string packageId,
        SourcePackageDependencyInfo[] dependencyInfos)
    {
        var resolverContext = new PackageResolverContext(
            DependencyBehavior.Lowest,
            new[] { packageId },
            Enumerable.Empty<string>(),
            Enumerable.Empty<PackageReference>(),
            Enumerable.Empty<PackageIdentity>(),
            dependencyInfos,
            _sourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
            _logger);

        var resolver = new PackageResolver();

        return resolver.Resolve(resolverContext, CancellationToken.None)
            .Select(p => dependencyInfos.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
    }

    private async Task CollectPackagesAsync(PackageIdentity package, ICollection<SourcePackageDependencyInfo> packages,
        CancellationToken cancellationToken)
    {
        if (packages.Contains(package)) return;

        foreach (var repository in _sourceRepositoryProvider.GetRepositories())
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);

            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                package,
                FrameworkConstants.CommonFrameworks.Net80,
                _cacheContext,
                _logger,
                cancellationToken);
            if (dependencyInfo == null) continue;

            var filteredSourceDependency = new SourcePackageDependencyInfo(
                dependencyInfo.Id,
                dependencyInfo.Version,
                dependencyInfo.Dependencies.Where(d => !IsDependencyProvided(d)),
                dependencyInfo.Listed,
                dependencyInfo.Source);

            packages.Add(filteredSourceDependency);

            foreach (var dependency in filteredSourceDependency.Dependencies)
            {
                await CollectPackagesAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                    packages, cancellationToken);
            }
        }
    }

    private async Task<PackageIdentity> GetPackageIdentityAsync(string packageId, bool allowPreview,
        CancellationToken cancellationToken)
    {
        foreach (var repository in _sourceRepositoryProvider.GetRepositories())
        {
            var packages = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            var versions = await packages.GetAllVersionsAsync(packageId, _cacheContext, _logger,
                cancellationToken);

            var latestVersion = versions.Where(v => allowPreview || !v.IsPrerelease).MaxBy(v => v);
            if (latestVersion == null) continue;

            return new PackageIdentity(packageId, latestVersion);
        }

        throw new InvalidOperationException($"Package '{packageId}' not found.");
    }

    private static bool IsDependencyProvided(PackageDependency dependency)
    {
        if (dependency.Id is "Doki" or "Doki.Abstractions" or "Doki.Output.Abstractions") return true;

        var runtimeLibrary = DependencyContext.Default!.RuntimeLibraries.FirstOrDefault(r => r.Name == dependency.Id);

        if (runtimeLibrary == null) return false;

        var parsedLibVersion = NuGetVersion.Parse(runtimeLibrary.Version);

        return parsedLibVersion.IsPrerelease || dependency.VersionRange.Satisfies(parsedLibVersion);
    }

    public void Dispose()
    {
        _cacheContext.Dispose();
    }
}