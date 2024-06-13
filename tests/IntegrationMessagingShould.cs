using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MT_Headers_Repro;
using Polly;
using Polly.Retry;

namespace MT_Headers_Repro_Tests;

public class IntegrationMessagingWithInMemoryShould
{

    string testedHeader = "MT-MessageType";

    [Fact]
    public async Task Expect_MessageTypeHeader_OnFirstMessageDelivery()
    {
        using var session = await CreateAndStartBus(
            setupConsumers: configurator =>
            {
                configurator.AddConsumer<FooConsumer>();
            });

        await SendMessageAndAssert(session);
    }


    [Fact]
    public async Task Expect_MessageTypeHeader_OnMessageRedelivery()
    {
        using var session = await CreateAndStartBus(
            setupConsumers: configurator =>
            {
                configurator.AddConsumer<FooConsumerWhichThrowsException>();
            });

        await SendMessageAndAssert(session);
    }

    private async Task SendMessageAndAssert(BusSession session)
    {
        // act
        using var scope = session.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var msg = new TestMessage("22");
        await session.PublishEndpoint(scope.ServiceProvider).Publish(msg);

        // assert
        await GetAsyncRetryPolicy().ExecuteAsync(async () =>
        {
            session.MessageProcessingCollector.Events
                .Any(x =>
                {
                    var header = x.Headers.Get<string>(testedHeader);
                    return header is "urn:message:MT_Headers_Repro_Tests:TestMessage";
                }).Should().BeTrue();
            await Task.CompletedTask;
        });
    }

    protected AsyncRetryPolicy GetAsyncRetryPolicy()
    {
        const int maxRetryCount = 5;
        var policy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(maxRetryCount,
            retryAttempt =>
                TimeSpan.FromMilliseconds(100 * Math.Pow(retryAttempt, 2)));
        return policy;
    }

    protected async Task<BusSession> CreateAndStartBus(
        Action<IServiceCollection> serviceBuilder = null,
        Action<IBusRegistrationConfigurator> setupConsumers = null,
        string boundedContextName = null)
    {
        BusSession session = null;

        try
        {
            var services = new ServiceCollection()
                .AddSingleton<TestMessageProcessingCollector>(); // needed for injection into the message handlers

            services.AddConfiguredMassTransit(
                setupConsumers);

            serviceBuilder?.Invoke(services);

            var serviceProvider = services.BuildServiceProvider();

            session = new BusSession(serviceProvider);

            await serviceProvider.GetRequiredService<IBusControl>().StartAsync();

            return session;
        }
        catch
        {
            session?.Dispose();
            throw;
        }
    }
}