using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public static class PlayerRegistryService
{
    private static readonly Dictionary<long, string> _knownPlayers = new();

    public static List<KeyValuePair<long, string>> GetKnownPlayers()
    {
        UpdateRegistry();

        List<KeyValuePair<long, string>> list = new(_knownPlayers);
        list.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.OrdinalIgnoreCase));
        return list;
    }

    public static void RegisterPlayer(long playerId, string playerName)
    {
        if (playerId == 0L || string.IsNullOrEmpty(playerName))
            return;

        _knownPlayers[playerId] = playerName;
    }

    private static void UpdateRegistry()
    {
        if (Player.m_localPlayer != null)
        {
            long localId = Player.m_localPlayer.GetPlayerID();
            string localName = Player.m_localPlayer.GetPlayerName();
            if (localId != 0L && !string.IsNullOrEmpty(localName))
            {
                _knownPlayers[localId] = localName;
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
                    _knownPlayers[id] = name;
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
                        _knownPlayers[id] = info.m_name;
                    }
                }
            }
        }

        List<KnownWard> allWards = WardRegistryManager.Instance.GetAllWards();
        for (int i = 0; i < allWards.Count; i++)
        {
            KnownWard ward = allWards[i];
            if (ward.CreatorId != 0L && !string.IsNullOrEmpty(ward.CreatorName))
            {
                _knownPlayers[ward.CreatorId] = ward.CreatorName;
            }

            if (ZDOMan.instance != null)
            {
                ZDO wardZdo = ZDOMan.instance.GetZDO(ward.Id);
                if (wardZdo != null)
                {
                    string mgrData = wardZdo.GetString(WardZdoKeys.RolesManagers, string.Empty);
                    if (!string.IsNullOrEmpty(mgrData))
                    {
                        WardRoster mgrRoster = WardRoster.Deserialize(mgrData);
                        for (int m = 0; m < mgrRoster.Entries.Count; m++)
                        {
                            RosterEntry entry = mgrRoster.Entries[m];
                            if (entry.PlayerId != 0L && !string.IsNullOrEmpty(entry.PlayerName))
                            {
                                _knownPlayers[entry.PlayerId] = entry.PlayerName;
                            }
                        }
                    }

                    string resData = wardZdo.GetString(WardZdoKeys.RolesResidents, string.Empty);
                    if (!string.IsNullOrEmpty(resData))
                    {
                        WardRoster resRoster = WardRoster.Deserialize(resData);
                        for (int r = 0; r < resRoster.Entries.Count; r++)
                        {
                            RosterEntry entry = resRoster.Entries[r];
                            if (entry.PlayerId != 0L && !string.IsNullOrEmpty(entry.PlayerName))
                            {
                                _knownPlayers[entry.PlayerId] = entry.PlayerName;
                            }
                        }
                    }
                }
            }
        }
    }
}
