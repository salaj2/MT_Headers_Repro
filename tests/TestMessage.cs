namespace MT_Headers_Repro_Tests;

public class TestMessage(string data)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Data { get; set; } = data;
}