using MassTransit;

namespace MT_Headers_Repro_Tests;

public class FooConsumer(TestMessageProcessingCollector collector) : IConsumer<TestMessage>
{
    public async Task Consume(ConsumeContext<TestMessage> context)
    {
        collector.HandleAndRecord(context.Message, context.ReceiveContext.TransportHeaders, context.MessageId?.ToString() ?? "", this);
        await Task.CompletedTask;
    }
}