using Doki.Output;
using Microsoft.Extensions.DependencyInjection;

namespace Doki;

internal class OutputBuilder(IServiceProvider? serviceProvider, IEnumerable<IOutput> outputs) : IAsyncDisposable
{
    private AsyncServiceScope? _scope;

    public IEnumerable<IOutput> Build()
    {
        _scope = serviceProvider?.CreateAsyncScope();

        if (_scope.HasValue)
        {
            var outputServices = _scope.Value.ServiceProvider.GetServices<IOutput>();

            foreach (var output in outputServices)
            {
                yield return output;
            }
        }

        foreach (var output in outputs)
        {
            yield return output;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_scope.HasValue) await _scope.Value.DisposeAsync();
    }
}