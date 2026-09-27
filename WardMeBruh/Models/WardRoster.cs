using System;
using System.Collections.Generic;
using System.Text;

namespace WardMeBruh.Models;

public class WardRoster
{
    private const char EntryDelimiter = ';';
    private const char FieldDelimiter = ':';

    private readonly List<RosterEntry> _entries = new();

    public IReadOnlyList<RosterEntry> Entries => _entries;

    public bool Contains(long playerId)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
                return true;
        }
        return false;
    }

    public void AddOrUpdate(long playerId, string playerName)
    {
        if (playerId == 0L)
            return;

        string safeName = string.IsNullOrEmpty(playerName) ? "Unknown" : playerName.Replace(EntryDelimiter, '_').Replace(FieldDelimiter, '_');

        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
            {
                _entries[i] = new RosterEntry(playerId, safeName);
                return;
            }
        }

        _entries.Add(new RosterEntry(playerId, safeName));
    }

    public void Add(long playerId, string playerName)
    {
        AddOrUpdate(playerId, playerName);
    }

    public bool Remove(long playerId)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
            {
                _entries.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public string Serialize()
    {
        if (_entries.Count == 0)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < _entries.Count; i++)
        {
            if (i > 0)
                builder.Append(EntryDelimiter);

            builder.Append(_entries[i].PlayerId);
            builder.Append(FieldDelimiter);
            builder.Append(_entries[i].PlayerName);
        }

        return builder.ToString();
    }

    public static WardRoster Deserialize(string serializedData)
    {
        WardRoster roster = new();
        if (string.IsNullOrWhiteSpace(serializedData))
            return roster;

        string[] tokens = serializedData.Split(new char[] { EntryDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            int splitIndex = token.IndexOf(FieldDelimiter);
            if (splitIndex <= 0)
                continue;

            string idPart = token.Substring(0, splitIndex);
            string namePart = token.Substring(splitIndex + 1);

            if (long.TryParse(idPart, out long parsedId))
            {
                roster.AddOrUpdate(parsedId, namePart);
            }
        }

        return roster;
    }
}
