using Annium.Logging;

namespace Annium.Testing;

public class TestingConfiguration
{
    public LogLevel LogLevel { get; }
    public string Filter { get; }

    public TestingConfiguration(
        LogLevel logLevel,
        string filter
    )
    {
        LogLevel = logLevel;
        Filter = filter;
    }
}