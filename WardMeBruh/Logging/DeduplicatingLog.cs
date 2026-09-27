using System;
using Vapok.Common.Abstractions;

namespace WardMeBruh.Logging;

public class DeduplicatingLog : ILogIt
{
    private readonly ILogIt _innerLog;
    private readonly object _lock = new();
    private string _lastDebugMessage = string.Empty;

    public DeduplicatingLog(ILogIt innerLog)
    {
        _innerLog = innerLog ?? throw new ArgumentNullException(nameof(innerLog));
    }

    public void Debug(string message)
    {
        if (message == null)
            return;

        lock (_lock)
        {
            if (string.Equals(message, _lastDebugMessage, StringComparison.Ordinal))
                return;

            _lastDebugMessage = message;
        }

        _innerLog.Debug(message);
    }

    public void Info(string message)
    {
        _innerLog.Info(message);
    }

    public void Message(string message)
    {
        _innerLog.Message(message);
    }

    public void Warning(string message)
    {
        _innerLog.Warning(message);
    }

    public void Error(string message)
    {
        _innerLog.Error(message);
    }

    public void Fatal(string message)
    {
        _innerLog.Fatal(message);
    }
}
