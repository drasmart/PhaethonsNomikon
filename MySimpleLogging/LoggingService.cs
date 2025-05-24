using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MySimpleLogging;

public sealed class LoggingService(Func<DateTime> dateTimeProvider) : ILoggerProvider
{
    private static readonly ObservableCollection<LogEntry> RawLogs = new();
    public static readonly ReadOnlyObservableCollection<LogEntry> Logs = new(RawLogs);
    private static readonly AsyncLocal<List<ScopeState>> Scopes = new();

    public static void ClearLogs() => RawLogs.Clear();
    
    public ILogger CreateLogger(string categoryName) => new Logger(dateTimeProvider);
    public void Dispose() {}

    private class Scope : IDisposable
    {
        public void Dispose()
        {
            Scopes.Value?.RemoveAt(Scopes.Value.Count - 1);
        }
    }

    private class ScopeState
    {
        public IEnumerable<string>? Names { get; init; }
        public JsonDocument? Values { get; init; }
        public bool Temporary { get; init; }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; init; }
        public LogLevel Level { get; init; }
        public string? Message { get; init; }
        public IEnumerable<KeyValuePair<string, object>>? Values { get; init; }
        public IEnumerable<string>? ScopeNames { get; init; }
    }

    private class Logger(Func<DateTime> dateTimeProvider) : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            using var scope = DoBeginScope(state, temporary: true);
            List<string> names = new();
            Dictionary<string, object> values = new();
            MergeScopes(names, values);
            var newEntry = new LogEntry
            {
                Timestamp = dateTimeProvider(),
                Level = logLevel,
                Message = formatter(state, exception),
                ScopeNames = names.Count > 0 ? names : null,
                Values = values.Count > 0 ? values : null,
            };
            lock (RawLogs)
            {
                RawLogs.Add(newEntry);
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => DoBeginScope(state, temporary: false);

        private IDisposable DoBeginScope<TState>(TState state, bool temporary)
        {
            Scopes.Value ??= [];
            Scopes.Value.Add(ToScopeState(state, temporary));
            return new Scope();
        }

        private static ScopeState ToScopeState<TState>(TState state, bool temporary) => state switch
        {
            string scopeName => new ScopeState
            {
                Names = [scopeName],
                Temporary = temporary,
            },
            IEnumerable<string> scopeNames => new ScopeState
            {
                Names = scopeNames.ToList(),
                Temporary = temporary,
            },
            IEnumerable<KeyValuePair<string, object>> e 
                when e.Select(kv => kv.Key).Contains("{OriginalFormat}") 
                => FromFormattedLogValue(e, temporary),
            _ => new ScopeState
            { 
                Values = JsonDocument.Parse(JsonSerializer.Serialize(state)),
                Temporary = temporary,
            }
        };

        private static ScopeState FromFormattedLogValue(
            IEnumerable<KeyValuePair<string, object>> state,
            bool temporary)
        {
            Dictionary<string, object> values = new();
            foreach (var entry in state.Where(kv => kv.Key != "{OriginalFormat}"))
            {
                values.Add(entry.Key, entry.Value);
            }
            return new ScopeState
            {
                Names = [state.ToString() ?? state.GetType().Name],
                Values = JsonDocument.Parse(JsonSerializer.Serialize(values)),
                Temporary = temporary,
            };
        }

        private static void MergeScopes(List<string> names, Dictionary<string, object> values)
        {
            foreach (var scopeState in Scopes.Value ?? [])
            {
                if (!scopeState.Temporary)
                {
                    foreach (var name in scopeState.Names ?? [])
                    {
                        names.Add(name);
                    }
                }

                if (scopeState.Values is {} valuesDoc)
                {
                    JsonElement root = valuesDoc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty prop in root.EnumerateObject())
                        {
                            values.Add(prop.Name, prop.Value.ToString());
                        }
                    }
                    else if (!scopeState.Temporary)
                    {
                        names.Add(root.ToString());
                    }
                }
            }
        }
    }
}