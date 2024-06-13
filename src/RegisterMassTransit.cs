using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace MT_Headers_Repro;

public static class MassTransitExtensions
{
    public static IServiceCollection AddConfiguredMassTransit(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator> setupConsumers = null,
        int mtHostShutdownTimeoutSeconds = 20)
    {
        services.AddMassTransit(mtConfig =>
        {
            // Register consumers and sagas
            setupConsumers?.Invoke(mtConfig);

            mtConfig.UsingInMemory((busRegistrationContext, busConfig) =>
            {
                busConfig.UseDelayedMessageScheduler();
                busConfig.UseDelayedRedelivery(redeliveryConfig =>
                {
                    redeliveryConfig.Exponential(2,
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(500));

                    busConfig.ClearSerialization();
                    busConfig.UseNewtonsoftRawJsonSerializer();
                    busConfig.UseNewtonsoftRawJsonDeserializer();
                });

                //ConfigureSerialization(busConfig);
                busConfig.ConfigureEndpoints(busRegistrationContext);
            });
        });

        services.Configure<MassTransitHostOptions>(x =>
        {
            x.WaitUntilStarted = true;
            x.StopTimeout = TimeSpan.FromSeconds(mtHostShutdownTimeoutSeconds);
        });

        return services;
    }
}