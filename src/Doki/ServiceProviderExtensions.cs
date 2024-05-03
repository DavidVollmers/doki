using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Doki;

internal static class ServiceProviderExtensions
{
    public static IServiceProvider With(this IServiceProvider serviceProvider, IServiceCollection serviceCollection)
    {
        var services = new ServiceCollection();

        foreach (var serviceDescriptor in serviceProvider.GetServiceDescriptors())
        {
            services.Add(serviceDescriptor);
        }

        foreach (var serviceDescriptor in serviceCollection)
        {
            services.Add(serviceDescriptor);
        }

        return services.BuildServiceProvider();
    }

    // https://stackoverflow.com/a/77427176/4382610
    private static IEnumerable<ServiceDescriptor> GetServiceDescriptors(this IServiceProvider serviceProvider)
    {
        while (true)
        {
            var originType = serviceProvider.GetType();
            if (originType.Name.Equals("ServiceProviderEngineScope"))
            {
                if (originType.GetProperty("RootProvider", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.GetValue(serviceProvider) is IServiceProvider sp)
                {
                    serviceProvider = sp;
                    continue;
                }

                yield break;
            }

            var callSiteFactory = serviceProvider.GetType()
                .GetProperty("CallSiteFactory", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(serviceProvider);
            var descriptors = callSiteFactory?.GetType()
                .GetField("_descriptors", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(callSiteFactory);
            if (descriptors is IEnumerable<ServiceDescriptor> refDescriptors)
            {
                foreach (var serviceDescriptor in refDescriptors)
                {
                    yield return serviceDescriptor;
                }
            }

            break;
        }
    }
}