using System.Collections.Generic;
using Jotunn.Managers;
using WardMeBruh.Configuration;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Models;

public static class WardPermissions
{
    public static bool IsDedicatedServerSession()
    {
        if (ZNet.instance == null)
            return false;

        if (ZNet.instance.IsDedicated() || GUIManager.IsHeadless())
            return true;

        return ZNet.instance.IsCurrentServerDedicated();
    }

    public static WardRole GetPlayerRole(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null || playerId == 0L)
            return WardRole.Guest;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return WardRole.Guest;

        if (IsAdmin(playerId))
            return WardRole.Owner;

        long creatorId = wardZdo.GetLong(ZDOVars.s_creator, 0L);
        if (creatorId != 0L && creatorId == playerId)
            return WardRole.Owner;

        string managersData = wardZdo.GetString(WardZdoKeys.RolesManagers, string.Empty);
        if (!string.IsNullOrEmpty(managersData))
        {
            WardRoster managersRoster = WardRoster.Deserialize(managersData);
            if (managersRoster.Contains(playerId))
                return WardRole.Manager;
        }

        string residentsData = wardZdo.GetString(WardZdoKeys.RolesResidents, string.Empty);
        if (!string.IsNullOrEmpty(residentsData))
        {
            WardRoster residentsRoster = WardRoster.Deserialize(residentsData);
            if (residentsRoster.Contains(playerId))
                return WardRole.Resident;
        }

        return WardRole.Guest;
    }

    public static bool HasRequiredRole(ZDO wardZdo, long playerId, WardRole minimumRequiredRole)
    {
        WardRole resolvedRole = GetPlayerRole(wardZdo, playerId);
        return resolvedRole >= minimumRequiredRole;
    }

    public static bool HasRequiredRole(PrivateArea ward, long playerId, WardRole minimumRequiredRole)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return false;

        return HasRequiredRole(ward.m_nview.GetZDO(), playerId, minimumRequiredRole);
    }

    public static bool HasAccess(PrivateArea ward, long playerId, WardRole minimumRequiredRole)
    {
        return HasRequiredRole(ward, playerId, minimumRequiredRole);
    }

    public static WardRole GetWardMinRole(ZDO wardZdo)
    {
        if (wardZdo == null)
            return WardRole.Guest;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return WardRole.Guest;

        int minRoleInt = wardZdo.GetInt(WardZdoKeys.WardMinRole, (int)WardRole.Guest);
        return (WardRole)minRoleInt;
    }

    public static bool HasWardAccess(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null)
            return true;

        return WardRoleManager.CanPerform(wardZdo, playerId, "core.dome_access");
    }

    public static bool HasWardAccess(PrivateArea ward, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return HasWardAccess(ward.m_nview.GetZDO(), playerId);
    }

    public static bool HasBenefactorAccess(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null)
            return true;

        return WardRoleManager.CanPerform(wardZdo, playerId, "core.benefits");
    }

    public static bool HasBenefactorAccess(PrivateArea ward, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return HasBenefactorAccess(ward.m_nview.GetZDO(), playerId);
    }

    public static bool CanBuild(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null)
            return true;

        return WardRoleManager.CanPerform(wardZdo, playerId, "core.building");
    }

    public static bool CanBuild(PrivateArea ward, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return CanBuild(ward.m_nview.GetZDO(), playerId);
    }

    public static bool CanFarm(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null)
            return true;

        return WardRoleManager.CanPerform(wardZdo, playerId, "core.farming");
    }

    public static bool CanFarm(PrivateArea ward, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return CanFarm(ward.m_nview.GetZDO(), playerId);
    }

    public static bool CanHarvest(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null)
            return true;

        return WardRoleManager.CanPerform(wardZdo, playerId, "core.harvest");
    }

    public static bool CanHarvest(PrivateArea ward, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return CanHarvest(ward.m_nview.GetZDO(), playerId);
    }

    public static int GetFeatureRoleKey(int featureKey)
    {
        if (featureKey == WardZdoKeys.GhostBase) return WardZdoKeys.GhostBaseMinRole;
        if (featureKey == WardZdoKeys.RaidSuppress) return WardZdoKeys.RaidSuppressMinRole;
        if (featureKey == WardZdoKeys.SpawnSuppress) return WardZdoKeys.SpawnSuppressMinRole;
        if (featureKey == WardZdoKeys.MaxComfort) return WardZdoKeys.MaxComfortMinRole;
        if (featureKey == WardZdoKeys.PortalAccess) return WardZdoKeys.PortalMinRole;
        if (featureKey == WardZdoKeys.RepairEnabled) return WardZdoKeys.RepairMinRole;
        if (featureKey == WardZdoKeys.AutoFuelEnabled) return WardZdoKeys.AutoFuelMinRole;
        if (featureKey == WardZdoKeys.AutoCloseDoors) return WardZdoKeys.AutoCloseDoorsMinRole;
        if (featureKey == WardZdoKeys.WeatherRoof) return WardZdoKeys.WeatherRoofMinRole;
        if (featureKey == WardZdoKeys.AshShield) return WardZdoKeys.AshShieldMinRole;
        if (featureKey == WardZdoKeys.LavaShield) return WardZdoKeys.LavaShieldMinRole;
        if (featureKey == WardZdoKeys.SnowShield) return WardZdoKeys.SnowShieldMinRole;
        if (featureKey == WardZdoKeys.WaterDrain) return WardZdoKeys.WaterDrainMinRole;
        if (featureKey == WardZdoKeys.RegenBoost) return WardZdoKeys.RegenBoostMinRole;
        if (featureKey == WardZdoKeys.DeathSkillShield) return WardZdoKeys.DeathSkillShieldMinRole;
        if (featureKey == WardZdoKeys.FoodFreeze) return WardZdoKeys.FoodFreezeMinRole;

        return 0;
    }

    public static WardRole GetFeatureMinRole(ZDO wardZdo, int featureKey)
    {
        if (wardZdo == null)
            return WardRole.Guest;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return WardRole.Guest;

        int roleKey = GetFeatureRoleKey(featureKey);
        if (roleKey == 0)
            return WardRole.Guest;

        int minRoleInt = wardZdo.GetInt(roleKey, (int)WardRole.Guest);
        return (WardRole)minRoleInt;
    }

    public static bool HasFeatureAccess(ZDO wardZdo, int featureKey, long playerId)
    {
        if (wardZdo == null)
            return true;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return true;

        if (playerId == 0L)
            return true;

        if (!HasBenefactorAccess(wardZdo, playerId))
            return false;

        WardRole minRole = GetFeatureMinRole(wardZdo, featureKey);
        return HasRequiredRole(wardZdo, playerId, minRole);
    }

    public static bool HasFeatureAccess(PrivateArea ward, int featureKey, long playerId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return true;

        return HasFeatureAccess(ward.m_nview.GetZDO(), featureKey, playerId);
    }

    public static bool CanManageWard(ZDO wardZdo, long playerId)
    {
        return HasRequiredRole(wardZdo, playerId, WardRole.Manager);
    }

    public static bool CanOpenDoor(ZDO doorZdo, ZDO wardZdo, long playerId)
    {
        if (doorZdo == null)
            return true;

        if (wardZdo == null)
            return true;

        int requiredRoleInt = doorZdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest);
        WardRole requiredRole = (WardRole)requiredRoleInt;

        return HasRequiredRole(wardZdo, playerId, requiredRole);
    }

    public static bool CanAccessContainer(ZDO containerZdo, ZDO wardZdo, long playerId)
    {
        if (containerZdo == null)
            return true;

        if (wardZdo == null)
            return true;

        int requiredRoleInt = containerZdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        WardRole requiredRole = (WardRole)requiredRoleInt;

        return HasRequiredRole(wardZdo, playerId, requiredRole);
    }

    public static bool IsAdmin(long playerId)
    {
        if (!IsDedicatedServerSession())
            return false;

        if (playerId == 0L)
            return false;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            if (ZNet.instance.m_adminList != null)
            {
                string idString = playerId.ToString();
                if (ZNet.instance.ListContainsId(ZNet.instance.m_adminList, idString))
                    return true;

                List<ZNetPeer> peers = ZNet.instance.GetPeers();
                for (int i = 0; i < peers.Count; i++)
                {
                    ZNetPeer peer = peers[i];
                    if (peer.IsReady() && NetPeerUtility.GetPeerPlayerId(peer.m_uid) == playerId)
                    {
                        string hostName = peer.m_socket != null ? peer.m_socket.GetHostName() : string.Empty;
                        if (!string.IsNullOrEmpty(hostName) && ZNet.instance.ListContainsId(ZNet.instance.m_adminList, hostName))
                            return true;
                    }
                }
            }

            return false;
        }

        if (Player.m_localPlayer != null && Player.m_localPlayer.GetPlayerID() == playerId)
        {
            if (ZNet.instance != null && ZNet.instance.PlayerIsAdmin(UserInfo.GetLocalUser().UserId))
                return true;

            if (SynchronizationManager.Instance != null && SynchronizationManager.Instance.PlayerIsAdmin)
                return true;
        }

        return false;
    }
}
