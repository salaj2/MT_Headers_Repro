using MassTransit;

namespace MT_Headers_Repro_Tests
{
    public class FooConsumerWhichThrowsException(TestMessageProcessingCollector collector) : IConsumer<TestMessage>
    {
        private static object _onlyOnce = new();

        public async Task Consume(ConsumeContext<TestMessage> context)
        {
            if (_onlyOnce != null)
            {
                _onlyOnce = null;
                throw new Exception("foo");
            }

            collector.HandleAndRecord(context.Message, context.ReceiveContext.TransportHeaders, context.MessageId?.ToString() ?? "", this);
            await Task.CompletedTask;
        }
    }
}
