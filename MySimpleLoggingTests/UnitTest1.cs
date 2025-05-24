using Microsoft.Extensions.Logging;
using MySimpleLogging;

namespace MySimpleLoggingTests;

public class Tests
{
    [SetUp]
    public void Setup() => LoggingService.ClearLogs();
    
    [TearDown]
    public void TearDown() => LoggingService.ClearLogs();

    [Test]
    public void LogInformation_String()
    {
        var now = DateTime.Now;
        const string msg = "test-message";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        logger.LogInformation(msg);

        var allEntries = LoggingService.Logs.ToList();
        Assert.That(allEntries, Has.Count.EqualTo(1));
        var entry = allEntries[0];
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
        Assert.That(entry.Timestamp, Is.EqualTo(now));
        Assert.That(entry.Message, Is.EqualTo(msg));
        Assert.That(entry.Values, Is.Null);
        Assert.That(entry.ScopeNames, Is.Null);
    }

    [Test]
    public void LogWarning_String()
    {
        var now = DateTime.Now;
        const string msg = "test-message";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        logger.LogWarning(msg);

        var allEntries = LoggingService.Logs.ToList();
        Assert.That(allEntries, Has.Count.EqualTo(1));
        var entry = allEntries[0];
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(entry.Timestamp, Is.EqualTo(now));
        Assert.That(entry.Message, Is.EqualTo(msg));
        Assert.That(entry.Values, Is.Null);
        Assert.That(entry.ScopeNames, Is.Null);
    }
}