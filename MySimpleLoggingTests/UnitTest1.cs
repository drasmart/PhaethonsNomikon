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
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(1));
            var entry = allEntries[0];
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.Message, Is.EqualTo(msg));
            Assert.That(entry.Values, Is.Null);
            Assert.That(entry.ScopeNames, Is.Null);
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(1));
            var entry = allEntries[0];
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.Message, Is.EqualTo(msg));
            Assert.That(entry.Values, Is.Null);
            Assert.That(entry.ScopeNames, Is.Null);
        });
    }

    [Test]
    public void LogInformation_String_ScopedByString()
    {
        var now = DateTime.Now;
        const string scope = "test-scope";
        const string msg = "test-message";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        using (logger.BeginScope(scope)) {
            logger.LogInformation(msg);
        }

        var allEntries = LoggingService.Logs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(1));
            var entry = allEntries[0];
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.Message, Is.EqualTo(msg));
            Assert.That(entry.Values, Is.Null);
            Assert.That(entry.ScopeNames, Is.Not.Null);
            var scopeNames = entry.ScopeNames!.ToList();
            Assert.That(scopeNames, Has.Count.EqualTo(1));
            Assert.That(scopeNames[0], Is.EqualTo(scope));
        });
    }

    [Test]
    public void LogInformation_TwoStrings_FirstScopedByString()
    {
        var now = DateTime.Now;
        const string scope1 = "test-scope";
        const string msg1 = "test-message-1";
        const string msg2 = "test-message-2";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        using (logger.BeginScope(scope1)) {
            logger.LogInformation(msg1);
        }
        logger.LogInformation(msg2);

        var allEntries = LoggingService.Logs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(2));
            var entry1 = allEntries[0];
            Assert.That(entry1.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry1.Timestamp, Is.EqualTo(now));
            Assert.That(entry1.Message, Is.EqualTo(msg1));
            Assert.That(entry1.Values, Is.Null);
            Assert.That(entry1.ScopeNames, Is.Not.Null);
            var scopeNames = entry1.ScopeNames!.ToList();
            Assert.That(scopeNames, Has.Count.EqualTo(1));
            Assert.That(scopeNames[0], Is.EqualTo(scope1));
            var entry2 = allEntries[1];
            Assert.That(entry2.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry2.Timestamp, Is.EqualTo(now));
            Assert.That(entry2.Message, Is.EqualTo(msg2));
            Assert.That(entry2.Values, Is.Null);
            Assert.That(entry2.ScopeNames, Is.Null);
        });
    }

    [Test]
    public void LogInformation_TwoStrings_SecondScopedByString()
    {
        var now = DateTime.Now;
        const string scope2 = "test-scope";
        const string msg1 = "test-message-1";
        const string msg2 = "test-message-2";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        logger.LogInformation(msg1);
        using (logger.BeginScope(scope2)) {
            logger.LogInformation(msg2);
        }

        var allEntries = LoggingService.Logs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(2));
            var entry1 = allEntries[0];
            Assert.That(entry1.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry1.Timestamp, Is.EqualTo(now));
            Assert.That(entry1.Message, Is.EqualTo(msg1));
            Assert.That(entry1.Values, Is.Null);
            Assert.That(entry1.ScopeNames, Is.Null);
            var entry2 = allEntries[1];
            Assert.That(entry2.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry2.Timestamp, Is.EqualTo(now));
            Assert.That(entry2.Message, Is.EqualTo(msg2));
            Assert.That(entry2.Values, Is.Null);
            Assert.That(entry2.ScopeNames, Is.Not.Null);
            var scopeNames = entry2.ScopeNames!.ToList();
            Assert.That(scopeNames, Has.Count.EqualTo(1));
            Assert.That(scopeNames[0], Is.EqualTo(scope2));
        });
    }

    [Test]
    public void LogInformation_String_ScopedByTwoStrings()
    {
        var now = DateTime.Now;
        const string scope1 = "test-scope1";
        const string scope2 = "test-scope2";
        const string msg = "test-message";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        using (logger.BeginScope(scope1))
        {
            using (logger.BeginScope(scope2))
            {
                logger.LogInformation(msg);
            }
        }

        var allEntries = LoggingService.Logs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(1));
            var entry = allEntries[0];
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.Message, Is.EqualTo(msg));
            Assert.That(entry.Values, Is.Null);
            Assert.That(entry.ScopeNames, Is.Not.Null);
            var scopeNames = entry.ScopeNames!.ToList();
            Assert.That(scopeNames, Has.Count.EqualTo(2));
            Assert.That(scopeNames[0], Is.EqualTo(scope1));
            Assert.That(scopeNames[1], Is.EqualTo(scope2));
        });
    }

    [Test]
    public void LogInformation_String_ScopedByDic()
    {
        var now = DateTime.Now;
        const string scopeKey = "test-key";
        const string scopeValue = "test-value";
        const string msg = "test-message";
        
        using LoggingService provider = new(() => now);
        var logger = provider.CreateLogger(nameof(Tests));
        using (logger.BeginScope(new Dictionary<string, string>{{scopeKey, scopeValue}})) {
            logger.LogInformation(msg);
        }

        var allEntries = LoggingService.Logs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(allEntries, Has.Count.EqualTo(1));
            var entry = allEntries[0];
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(entry.Timestamp, Is.EqualTo(now));
            Assert.That(entry.Message, Is.EqualTo(msg));
            Assert.That(entry.ScopeNames, Is.Null);
            Assert.That(entry.Values, Is.Not.Null);
            var values = entry.Values!.ToList();
            Assert.That(values, Has.Count.EqualTo(1));
            Assert.That(values[0].Key, Is.EqualTo(scopeKey));
            Assert.That(values[0].Value, Is.EqualTo(scopeValue));
        });
    }
}