using System;
using System.Collections.Generic;
using System.Text;

namespace WardMeBruh.Models;

public class WardRoleAssignments
{
    private const char EntryDelimiter = ';';
    private const char FieldDelimiter = ':';

    private readonly List<PlayerRoleAssignment> _entries = new();

    public IReadOnlyList<PlayerRoleAssignment> Entries => _entries;

    public bool Contains(long playerId)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
                return true;
        }
        return false;
    }

    public int GetRoleId(long playerId, int defaultRoleId = 0)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
                return _entries[i].RoleId;
        }
        return defaultRoleId;
    }

    public void SetRole(long playerId, string playerName, int roleId)
    {
        if (playerId == 0L)
            return;

        string safeName = string.IsNullOrEmpty(playerName) ? "Unknown" : playerName.Replace(EntryDelimiter, '_').Replace(FieldDelimiter, '_');

        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].PlayerId == playerId)
            {
                _entries[i] = new PlayerRoleAssignment(playerId, roleId, safeName);
                return;
            }
        }

        _entries.Add(new PlayerRoleAssignment(playerId, roleId, safeName));
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
            builder.Append(_entries[i].RoleId);
            builder.Append(FieldDelimiter);
            builder.Append(_entries[i].PlayerName);
        }

        return builder.ToString();
    }

    public static WardRoleAssignments Deserialize(string serializedData)
    {
        WardRoleAssignments assignments = new();
        if (string.IsNullOrWhiteSpace(serializedData))
            return assignments;

        string[] tokens = serializedData.Split(new char[] { EntryDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            string[] parts = token.Split(FieldDelimiter);
            if (parts.Length < 3)
                continue;

            if (long.TryParse(parts[0], out long parsedId) && int.TryParse(parts[1], out int parsedRoleId))
            {
                string namePart = parts[2];
                assignments.SetRole(parsedId, namePart, parsedRoleId);
            }
        }

        return assignments;
    }

    public void SyncFromLegacy(WardRoster managers, WardRoster residents)
    {
        if (managers != null)
        {
            for (int i = 0; i < managers.Entries.Count; i++)
            {
                RosterEntry entry = managers.Entries[i];
                SetRole(entry.PlayerId, entry.PlayerName, (int)WardRole.Manager);
            }
        }

        if (residents != null)
        {
            for (int i = 0; i < residents.Entries.Count; i++)
            {
                RosterEntry entry = residents.Entries[i];
                if (!Contains(entry.PlayerId))
                {
                    SetRole(entry.PlayerId, entry.PlayerName, (int)WardRole.Resident);
                }
            }
        }
    }

    public void SyncToLegacy(out WardRoster managers, out WardRoster residents)
    {
        managers = new WardRoster();
        residents = new WardRoster();

        for (int i = 0; i < _entries.Count; i++)
        {
            PlayerRoleAssignment entry = _entries[i];
            if (entry.RoleId == (int)WardRole.Manager)
            {
                managers.AddOrUpdate(entry.PlayerId, entry.PlayerName);
            }
            else if (entry.RoleId == (int)WardRole.Resident)
            {
                residents.AddOrUpdate(entry.PlayerId, entry.PlayerName);
            }
        }
    }
}
