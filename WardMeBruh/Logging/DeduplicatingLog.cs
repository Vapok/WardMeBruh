using System;
using System.Collections.Generic;
using Vapok.Common.Abstractions;

namespace WardMeBruh.Logging;

public class DeduplicatingLog : ILogIt
{
    private readonly ILogIt _innerLog;
    private readonly object _lock = new();
    private readonly HashSet<string> _messagesThisWindow = new(StringComparer.Ordinal);
    private int _lastTimestamp;
    private const int WindowMs = 50;

    public DeduplicatingLog(ILogIt innerLog)
    {
        _innerLog = innerLog ?? throw new ArgumentNullException(nameof(innerLog));
    }

    public void Debug(string message)
    {
        if (message == null)
            return;

        int now = Environment.TickCount;
        lock (_lock)
        {
            if (now - _lastTimestamp > WindowMs)
            {
                _lastTimestamp = now;
                _messagesThisWindow.Clear();
            }

            if (!_messagesThisWindow.Add(message))
                return;
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
