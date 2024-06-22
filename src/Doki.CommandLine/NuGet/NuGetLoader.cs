using System.Reflection;
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

        var currentVersion = NuGetVersion.Parse(typeof(DocumentationObject).Assembly.GetName().Version!.ToString());

        var packageIdentities = await GetPackageIdentitiesAsync(packageId, allowPreview, cancellationToken);
        foreach (var packageIdentity in packageIdentities)
        {
            var dependencyInfos = new HashSet<SourcePackageDependencyInfo>();
            var result = await ScanPackagesAsync(currentVersion, packageIdentity, dependencyInfos, cancellationToken);
            if (!result) continue;

            var packagesToInstall = GetPackagesToInstall(packageId, dependencyInfos.ToArray());

            var settings = Settings.LoadDefaultSettings(destinationDirectory);

            await InstallPackagesAsync(packagesToInstall, destinationDirectory, settings, cancellationToken);

            return Path.Combine(destinationDirectory, $"{packageIdentity.Id}.{packageIdentity.Version}", "lib",
                "net8.0",
                $"{packageIdentity.Id}.dll");
        }

        throw new InvalidOperationException($"No applicable package '{packageId}' found for version: {currentVersion}");
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
            [],
            [],
            [],
            dependencyInfos,
            _sourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
            _logger);

        var resolver = new PackageResolver();

        return resolver.Resolve(resolverContext, CancellationToken.None)
            .Select(p => dependencyInfos.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
    }

    private async Task<bool> ScanPackagesAsync(NuGetVersion currentVersion, PackageIdentity package,
        ICollection<SourcePackageDependencyInfo> packages, CancellationToken cancellationToken)
    {
        if (packages.Contains(package)) return true;

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

            var dependencies = new List<PackageDependency>();
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                if (dependency.Id is "Doki" or "Doki.Abstractions" or "Doki.Output.Extensions"
                    or "Doki.Output.Abstractions")
                {
                    if (dependency.VersionRange.Satisfies(currentVersion)) continue;

                    _logger.LogDebug(
                        $"Doki dependency '{dependency.Id}' version '{dependency.VersionRange}' does not satisfy current version '{currentVersion}'");

                    return false;
                }

                if (!IsDependencyProvided(dependency)) dependencies.Add(dependency);
            }

            var filteredSourceDependency = new SourcePackageDependencyInfo(
                dependencyInfo.Id,
                dependencyInfo.Version,
                dependencies,
                dependencyInfo.Listed,
                dependencyInfo.Source);

            packages.Add(filteredSourceDependency);

            foreach (var dependency in dependencies)
            {
                var result = await ScanPackagesAsync(currentVersion,
                    new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), packages,
                    cancellationToken);
                if (!result) return false;
            }
        }

        return true;
    }

    private async Task<IEnumerable<PackageIdentity>> GetPackageIdentitiesAsync(string packageId, bool allowPreview,
        CancellationToken cancellationToken)
    {
        foreach (var repository in _sourceRepositoryProvider.GetRepositories())
        {
            var packages = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            var versions = await packages.GetAllVersionsAsync(packageId, _cacheContext, _logger,
                cancellationToken);

            var allowedVersions = versions.Where(v => allowPreview || !v.IsPrerelease).ToArray();

            if (allowedVersions.Length != 0)
                return allowedVersions
                    .OrderByDescending(v => v)
                    .Select(v => new PackageIdentity(packageId, v));
        }

        throw new InvalidOperationException($"Package '{packageId}' not found.");
    }

    private static bool IsDependencyProvided(PackageDependency dependency)
    {
        var runtimeLibrary = DependencyContext.Default!.RuntimeLibraries.FirstOrDefault(r => r.Name == dependency.Id);

        if (runtimeLibrary == null) return false;

        var parsedLibVersion = NuGetVersion.Parse(runtimeLibrary.Version);

        return dependency.VersionRange.Satisfies(parsedLibVersion);
    }

    public void Dispose()
    {
        _cacheContext.Dispose();
    }
}