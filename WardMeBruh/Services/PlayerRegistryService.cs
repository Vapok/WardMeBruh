using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jotunn.Managers;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;

namespace WardMeBruh.Services;

public static class PlayerRegistryService
{
    private static readonly Dictionary<long, string> _knownPlayers = new();
    private static string _currentWorldName = string.Empty;

    public static List<KeyValuePair<long, string>> GetKnownPlayers()
    {
        UpdateRegistry();

        List<KeyValuePair<long, string>> list = new(_knownPlayers);
        list.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.OrdinalIgnoreCase));
        return list;
    }

    public static Dictionary<long, string> GetAllRegisteredPlayers()
    {
        return new Dictionary<long, string>(_knownPlayers);
    }

    public static void RegisterPlayer(long playerId, string playerName)
    {
        RegisterAndPersistPlayer(playerId, playerName, true);
    }

    public static void RegisterAndPersistPlayer(long playerId, string playerName, bool broadcast = true)
    {
        if (playerId == 0L || string.IsNullOrEmpty(playerName))
            return;

        bool isNewOrChanged = !_knownPlayers.TryGetValue(playerId, out string existingName) || existingName != playerName;
        _knownPlayers[playerId] = playerName;

        if (isNewOrChanged)
        {
            if (ZNet.instance == null || ZNet.instance.IsServer())
            {
                SaveWorld();
                if (broadcast && ZNet.instance != null && ZNet.instance.IsServer())
                {
                    WardNetworkRouter.BroadcastPlayer(playerId, playerName);
                }
            }
        }
    }

    public static void LoadWorld(string worldName)
    {
        if (string.IsNullOrEmpty(worldName))
            worldName = "default";

        _currentWorldName = worldName;
        string filePath = GetStorageFilePath(worldName);
        if (!File.Exists(filePath))
            return;

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("\"id\"") && line.Contains("\"name\""))
                {
                    long id = 0L;
                    string name = string.Empty;

                    int idIdx = line.IndexOf("\"id\"", StringComparison.OrdinalIgnoreCase);
                    if (idIdx >= 0)
                    {
                        int colonIdx = line.IndexOf(':', idIdx);
                        if (colonIdx >= 0)
                        {
                            int commaIdx = line.IndexOf(',', colonIdx);
                            string idStr = (commaIdx >= 0 ? line.Substring(colonIdx + 1, commaIdx - colonIdx - 1) : line.Substring(colonIdx + 1)).Trim();
                            long.TryParse(idStr, out id);
                        }
                    }

                    int nameIdx = line.IndexOf("\"name\"", StringComparison.OrdinalIgnoreCase);
                    if (nameIdx >= 0)
                    {
                        int colonIdx = line.IndexOf(':', nameIdx);
                        if (colonIdx >= 0)
                        {
                            int quoteStart = line.IndexOf('"', colonIdx);
                            if (quoteStart >= 0)
                            {
                                int quoteEnd = line.LastIndexOf('"');
                                if (quoteEnd > quoteStart)
                                {
                                    name = line.Substring(quoteStart + 1, quoteEnd - quoteStart - 1).Replace("\\\"", "\"").Replace("\\\\", "\\");
                                }
                            }
                        }
                    }

                    if (id != 0L && !string.IsNullOrEmpty(name))
                    {
                        _knownPlayers[id] = name;
                    }
                }
            }
            WardMeBruh.Log.Debug($"[PlayerRegistry] Loaded {_knownPlayers.Count} known players for world '{worldName}'");
        }
        catch (Exception ex)
        {
            WardMeBruh.Log.Warning($"[PlayerRegistry] Error loading saved players for world {worldName}: {ex.Message}");
        }
    }

    public static void SaveWorld()
    {
        if (_knownPlayers.Count == 0)
            return;

        if (string.IsNullOrEmpty(_currentWorldName))
        {
            if (ZNet.instance != null && !string.IsNullOrEmpty(ZNet.instance.GetWorldName()))
            {
                _currentWorldName = ZNet.instance.GetWorldName();
            }
            else
            {
                _currentWorldName = "default";
            }
        }

        try
        {
            string dir = Path.Combine(BepInEx.Paths.ConfigPath, "WardMeBruh", "Players");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string filePath = GetStorageFilePath(_currentWorldName);
            StringBuilder sb = new();
            sb.AppendLine("{");
            sb.AppendLine("  \"players\": [");
            int count = 0;
            foreach (KeyValuePair<long, string> kvp in _knownPlayers)
            {
                if (count > 0)
                    sb.AppendLine(",");

                string safeName = kvp.Value.Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append($"    {{ \"id\": {kvp.Key}, \"name\": \"{safeName}\" }}");
                count++;
            }
            sb.AppendLine();
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString());
        }
        catch (Exception ex)
        {
            WardMeBruh.Log.Warning($"[PlayerRegistry] Error saving players for world {_currentWorldName}: {ex.Message}");
        }
    }

    private static string GetStorageFilePath(string worldName)
    {
        string dir = Path.Combine(BepInEx.Paths.ConfigPath, "WardMeBruh", "Players");
        string safeName = SanitizeFileName(worldName);
        return Path.Combine(dir, $"{safeName}.json");
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalid.Length; i++)
        {
            name = name.Replace(invalid[i], '_');
        }
        return string.IsNullOrEmpty(name) ? "default" : name;
    }

    private static void UpdateRegistry()
    {
        bool changed = false;

        if (Player.m_localPlayer != null)
        {
            long localId = Player.m_localPlayer.GetPlayerID();
            string localName = Player.m_localPlayer.GetPlayerName();
            if (localId != 0L && !string.IsNullOrEmpty(localName))
            {
                if (!_knownPlayers.TryGetValue(localId, out string existing) || existing != localName)
                {
                    _knownPlayers[localId] = localName;
                    changed = true;
                }
            }
        }

        List<Player> activePlayers = Player.GetAllPlayers();
        for (int i = 0; i < activePlayers.Count; i++)
        {
            Player p = activePlayers[i];
            if (p != null)
            {
                long id = p.GetPlayerID();
                string name = p.GetPlayerName();
                if (id != 0L && !string.IsNullOrEmpty(name))
                {
                    if (!_knownPlayers.TryGetValue(id, out string existing) || existing != name)
                    {
                        _knownPlayers[id] = name;
                        changed = true;
                    }
                }
            }
        }

        if (ZNet.instance != null)
        {
            List<ZNet.PlayerInfo> netPlayers = ZNet.instance.GetPlayerList();
            for (int i = 0; i < netPlayers.Count; i++)
            {
                ZNet.PlayerInfo info = netPlayers[i];
                if (!info.m_characterID.IsNone())
                {
                    long id = 0L;
                    if (ZDOMan.instance != null)
                    {
                        ZDO zdo = ZDOMan.instance.GetZDO(info.m_characterID);
                        if (zdo != null)
                        {
                            id = zdo.GetLong(ZDOVars.s_playerID, 0L);
                        }
                    }

                    if (id == 0L)
                    {
                        id = info.m_characterID.UserID;
                    }

                    if (id != 0L && !string.IsNullOrEmpty(info.m_name))
                    {
                        if (!_knownPlayers.TryGetValue(id, out string existing) || existing != info.m_name)
                        {
                            _knownPlayers[id] = info.m_name;
                            changed = true;
                        }
                    }
                }
            }
        }

        bool isServerOrLocal = ZNet.instance == null || ZNet.instance.IsServer();
        if (isServerOrLocal && !GUIManager.IsHeadless())
        {
            try
            {
                List<PlayerProfile> profiles = SaveSystem.GetAllPlayerProfiles();
                if (profiles != null)
                {
                    for (int i = 0; i < profiles.Count; i++)
                    {
                        PlayerProfile profile = profiles[i];
                        if (profile != null)
                        {
                            long id = profile.GetPlayerID();
                            string name = profile.GetName();
                            if (id != 0L && !string.IsNullOrEmpty(name))
                            {
                                if (!_knownPlayers.TryGetValue(id, out string existing) || existing != name)
                                {
                                    _knownPlayers[id] = name;
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WardMeBruh.Log.Warning($"[PlayerRegistry] Error loading player profiles: {ex.Message}");
            }
        }

        List<KnownWard> allWards = WardRegistryManager.Instance.GetAllWards();
        for (int i = 0; i < allWards.Count; i++)
        {
            KnownWard ward = allWards[i];
            if (ward.CreatorId != 0L && !string.IsNullOrEmpty(ward.CreatorName))
            {
                if (!_knownPlayers.TryGetValue(ward.CreatorId, out string existing) || existing != ward.CreatorName)
                {
                    _knownPlayers[ward.CreatorId] = ward.CreatorName;
                    changed = true;
                }
            }

            if (ZDOMan.instance != null)
            {
                ZDO wardZdo = ZDOMan.instance.GetZDO(ward.Id);
                if (wardZdo != null)
                {
                    WardRoleAssignments assignments = WardRoleManager.GetRoleAssignments(wardZdo);
                    for (int a = 0; a < assignments.Entries.Count; a++)
                    {
                        PlayerRoleAssignment entry = assignments.Entries[a];
                        if (entry.PlayerId != 0L && !string.IsNullOrEmpty(entry.PlayerName))
                        {
                            if (!_knownPlayers.TryGetValue(entry.PlayerId, out string existing) || existing != entry.PlayerName)
                            {
                                _knownPlayers[entry.PlayerId] = entry.PlayerName;
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        if (changed)
        {
            SaveWorld();
        }
    }
}
