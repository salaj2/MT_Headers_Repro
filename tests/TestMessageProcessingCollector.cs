using MassTransit;

namespace MT_Headers_Repro_Tests;

/// <summary>
/// Collects start/stop messages and provides some trivial infrastructure for sequence testing.
/// </summary>
public class TestMessageProcessingCollector
{
    private readonly List<LogEvent> _events = new();

    #region Recording

    public class LogEvent
    {
        public DateTime TimeStamp;
        public string LogMessage;
        public TestMessage Message;
        public Headers Headers;
        public string MessageId;
        public object Handler;
        public Type EntityType;

        public override string ToString() => $"{LogMessage} (id {Message.Id} received on {TimeStamp} by handler {Handler})";
    }

    public List<LogEvent> Events
    {
        get
        {
            lock (_events)
                return _events.ToList();
        }
    }

    public void AddRecord(string msg, TestMessage message, Headers headers, string messageId, object handler, Type entityType = null)
    {
        var now = DateTime.Now;

        lock (_events)
            _events.Add(new LogEvent { TimeStamp = now, LogMessage = msg, Message = message, Headers = headers, MessageId = messageId, Handler = handler, EntityType = entityType});
    }

    public Func<TestMessage, object, string> DoneMessageFormatter = (msg, handler) => $"{msg.GetType().Name}-{msg.Data}-done";

    internal void HandleAndRecord(TestMessage message, Headers headers, string messageId, object handler, Type entityType = null)
    {
        AddRecord(DoneMessageFormatter(message, handler), message, headers, messageId, handler, entityType);
    }

    #endregion
}
