using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace MT_Headers_Repro_Tests;

/// <summary>
///     Holds a running bus instance and all the tooling to use it for testing against.
/// </summary>
public sealed class BusSession : IDisposable
{
    public ServiceProvider ServiceProvider { get; init; }

    /// <summary>
    ///     Use this to assert on invoked message handlers (the FitXP abstraction layer on top of MassTransit).
    /// </summary>
    public TestMessageProcessingCollector MessageProcessingCollector =>
        ServiceProvider.GetRequiredService<TestMessageProcessingCollector>();

    public IPublishEndpoint PublishEndpoint(IServiceProvider serviceProvider = null) =>
        (serviceProvider ?? ServiceProvider).GetRequiredService<IPublishEndpoint>();

    /// <summary>
    ///     Is called when this bus instance is disposed.
    /// </summary>
    public event Action OnDisposed;

    #region Construction / Disposal

    public BusSession(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void Dispose()
    {

        ServiceProvider.GetService<IBusControl>()?.Stop();

        OnDisposed?.Invoke();

        ServiceProvider.Dispose();
    }

    #endregion
}