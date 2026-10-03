using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Networking;

public static class WardNetworkRouter
{
    private const string RpcSyncRegistry = "WMB_SyncRegistry";
    private const string RpcRequestSync = "WMB_RequestSync";
    private const string RpcTransferOwnership = "WMB_TransferOwnership";
    private const string RpcSetEntityName = "WMB_SetEntityName";
    private const string RpcSetDoorRole = "WMB_SetDoorRole";
    private const string RpcSetContainerRole = "WMB_SetContainerRole";
    private const string RpcSetContainerFuel = "WMB_SetContainerFuel";
    private const string RpcSetPortalRole = "WMB_SetPortalRole";
    private const string RpcSetWardRole = "WMB_SetWardRole";
    private const string RpcSetFeatureRole = "WMB_SetFeatureRole";
    private const string RpcSetWardBool = "WMB_SetWardBool";
    private const string RpcSetWardFloat = "WMB_SetWardFloat";
    private const string RpcRegisterWard = "WMB_RegisterWard";
    private const string RpcSetRoleDefinitions = "WMB_SetRoleDefinitions";
    private const string RpcSetRoleAssignments = "WMB_SetRoleAssignments";
    private const string RpcSyncAllPlayers = "WMB_SyncAllPlayers";
    private const string RpcSyncSinglePlayer = "WMB_SyncSinglePlayer";
    private const string RpcRegisterPlayer = "WMB_RegisterPlayer";
    private const string RpcUnregisterWard = "WMB_UnregisterWard";

    public static void Register()
    {
        if (ZRoutedRpc.instance == null)
            return;

        ZRoutedRpc.instance.Register(RpcSyncRegistry, new Action<long, ZPackage>(RPC_SyncRegistry));
        ZRoutedRpc.instance.Register(RpcRequestSync, new Action<long>(RPC_RequestSync));
        ZRoutedRpc.instance.Register(RpcTransferOwnership, new Action<long, ZPackage>(RPC_TransferOwnership));
        ZRoutedRpc.instance.Register(RpcSetEntityName, new Action<long, ZPackage>(RPC_SetEntityName));
        ZRoutedRpc.instance.Register(RpcSetDoorRole, new Action<long, ZPackage>(RPC_SetDoorRole));
        ZRoutedRpc.instance.Register(RpcSetContainerRole, new Action<long, ZPackage>(RPC_SetContainerRole));
        ZRoutedRpc.instance.Register(RpcSetContainerFuel, new Action<long, ZPackage>(RPC_SetContainerFuel));
        ZRoutedRpc.instance.Register(RpcSetPortalRole, new Action<long, ZPackage>(RPC_SetPortalRole));
        ZRoutedRpc.instance.Register(RpcSetWardRole, new Action<long, ZPackage>(RPC_SetWardRole));
        ZRoutedRpc.instance.Register(RpcSetFeatureRole, new Action<long, ZPackage>(RPC_SetFeatureRole));
        ZRoutedRpc.instance.Register(RpcSetWardBool, new Action<long, ZPackage>(RPC_SetWardBool));
        ZRoutedRpc.instance.Register(RpcSetWardFloat, new Action<long, ZPackage>(RPC_SetWardFloat));
        ZRoutedRpc.instance.Register(RpcRegisterWard, new Action<long, ZPackage>(RPC_RegisterWard));
        ZRoutedRpc.instance.Register(RpcSetRoleDefinitions, new Action<long, ZPackage>(RPC_SetRoleDefinitions));
        ZRoutedRpc.instance.Register(RpcSetRoleAssignments, new Action<long, ZPackage>(RPC_SetRoleAssignments));
        ZRoutedRpc.instance.Register(RpcSyncAllPlayers, new Action<long, ZPackage>(RPC_SyncAllPlayers));
        ZRoutedRpc.instance.Register(RpcSyncSinglePlayer, new Action<long, ZPackage>(RPC_SyncSinglePlayer));
        ZRoutedRpc.instance.Register(RpcRegisterPlayer, new Action<long, ZPackage>(RPC_RegisterPlayer));
        ZRoutedRpc.instance.Register(RpcUnregisterWard, new Action<long, ZPackage>(RPC_UnregisterWard));
    }

    public static void RequestRegistrySync()
    {
        if (ZRoutedRpc.instance == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            return;
        }

        WardMeBruh.Log.Debug("[Network] Transmitting ward registry sync request to server");
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcRequestSync);
    }

    public static void BroadcastRegistry()
    {
        if (ZRoutedRpc.instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        WardMeBruh.Log.Debug($"[Network] Broadcasting ward registry to all peers ({WardRegistryManager.Instance.Count} wards)");
        ZPackage pkg = WardRegistryManager.Instance.Pack();
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcSyncRegistry, pkg);
    }

    public static void SendTransferOwnership(ZDOID wardId, long newOwnerId, string newOwnerName)
    {
        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting ownership transfer: Ward {wardId} -> New Owner: {newOwnerName} ({newOwnerId})");

        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(newOwnerId);
        pkg.Write(newOwnerName ?? string.Empty);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcTransferOwnership, pkg);
    }

    public static void SendUnregisterWard(ZDOID wardId)
    {
        if (ZRoutedRpc.instance == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            WardRegistryManager.Instance.Unregister(wardId);
            BroadcastRegistry();
            return;
        }

        WardMeBruh.Log.Debug($"[Network] Transmitting unregister ward request: Ward {wardId}");
        ZPackage pkg = new();
        pkg.Write(wardId);
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcUnregisterWard, pkg);
    }

    public static void SendEntityName(ZDOID entityId, int keyHash, string newName)
    {
        if (ZDOMan.instance != null)
        {
            ZDO entityZdo = ZDOMan.instance.GetZDO(entityId);
            if (entityZdo != null)
            {
                entityZdo.Set(keyHash, newName ?? string.Empty);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting entity rename: {entityId}, KeyHash: {keyHash}, NewName: '{newName}'");

        ZPackage pkg = new();
        pkg.Write(entityId);
        pkg.Write(keyHash);
        pkg.Write(newName ?? string.Empty);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetEntityName, pkg);
    }

    public static void SendDoorRole(ZDOID doorId, int minRole)
    {
        if (ZDOMan.instance != null)
        {
            ZDO doorZdo = ZDOMan.instance.GetZDO(doorId);
            if (doorZdo != null)
            {
                doorZdo.Set(WardZdoKeys.DoorMinRole, minRole);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting door clearance role: {doorId} -> MinRole: {(WardRole)minRole}");

        ZPackage pkg = new();
        pkg.Write(doorId);
        pkg.Write(minRole);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetDoorRole, pkg);
    }

    public static void SendContainerRole(ZDOID containerId, int minRole)
    {
        if (ZDOMan.instance != null)
        {
            ZDO containerZdo = ZDOMan.instance.GetZDO(containerId);
            if (containerZdo != null)
            {
                containerZdo.Set(WardZdoKeys.ContainerMinRole, minRole);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting container clearance role: {containerId} -> MinRole: {(WardRole)minRole}");

        ZPackage pkg = new();
        pkg.Write(containerId);
        pkg.Write(minRole);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetContainerRole, pkg);
    }

    public static void SendContainerFuel(ZDOID containerId, bool isFuelDepot)
    {
        if (ZDOMan.instance != null)
        {
            ZDO containerZdo = ZDOMan.instance.GetZDO(containerId);
            if (containerZdo != null)
            {
                containerZdo.Set(WardZdoKeys.ContainerIsFuel, isFuelDepot);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting container fuel depot setting: {containerId} -> IsFuelDepot: {isFuelDepot}");

        ZPackage pkg = new();
        pkg.Write(containerId);
        pkg.Write(isFuelDepot);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetContainerFuel, pkg);
    }

    public static void SendPortalRole(ZDOID portalId, int minRole)
    {
        if (ZDOMan.instance != null)
        {
            ZDO portalZdo = ZDOMan.instance.GetZDO(portalId);
            if (portalZdo != null)
            {
                portalZdo.Set(WardZdoKeys.PortalMinRole, minRole);
                ZDOMan.instance.SetDirtyPortals();
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting portal clearance role: {portalId} -> MinRole: {(WardRole)minRole}");

        ZPackage pkg = new();
        pkg.Write(portalId);
        pkg.Write(minRole);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetPortalRole, pkg);
    }

    public static void SendWardRole(ZDOID wardId, int minRole)
    {
        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting ward clearance role: {wardId} -> MinRole: {(WardRole)minRole}");

        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(minRole);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetWardRole, pkg);

        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(WardZdoKeys.WardMinRole, minRole);
            }
        }
    }

    public static void SendFeatureRole(ZDOID wardId, int featureRoleKey, int minRole)
    {
        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting feature clearance role: {wardId}, Key: {featureRoleKey} -> MinRole: {(WardRole)minRole}");

        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(featureRoleKey);
        pkg.Write(minRole);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetFeatureRole, pkg);

        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(featureRoleKey, minRole);
            }
        }
    }

    public static void SendWardBool(ZDOID wardId, int keyHash, bool value)
    {
        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(keyHash, value);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting ward bool setting: Ward {wardId}, Key: {keyHash} -> Value: {value}");

        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(keyHash);
        pkg.Write(value);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetWardBool, pkg);
    }

    public static void SendWardFloat(ZDOID wardId, int keyHash, float value)
    {
        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(keyHash, value);
            }
        }

        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting ward float setting: Ward {wardId}, Key: {keyHash} -> Value: {value}");

        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(keyHash);
        pkg.Write(value);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetWardFloat, pkg);
    }

    public static void SendRegisterWard(ZDOID wardId)
    {
        if (ZRoutedRpc.instance == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            if (ZDOMan.instance != null)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(wardId);
                if (zdo != null)
                {
                    WardRegistryManager.Instance.RegisterOrUpdate(zdo);
                    BroadcastRegistry();
                }
            }
            return;
        }

        WardMeBruh.Log.Debug($"[Network] Transmitting register ward: Ward {wardId}");
        ZPackage pkg = new();
        pkg.Write(wardId);

        long serverPeer = ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(serverPeer, RpcRegisterWard, pkg);
    }

    public static void SendRenameWard(ZDOID wardId, string newName)
    {
        SendEntityName(wardId, WardZdoKeys.CustomName, newName);
    }

    public static void SendRenameDoor(ZDOID doorId, string newName)
    {
        SendEntityName(doorId, WardZdoKeys.DoorName, newName);
    }

    public static void SendRenameContainer(ZDOID containerId, string newName)
    {
        SendEntityName(containerId, WardZdoKeys.ContainerName, newName);
    }

    public static void SendRenameEntity(ZDOID entityId, string newName, bool isDoor = false)
    {
        SendEntityName(entityId, isDoor ? WardZdoKeys.DoorName : WardZdoKeys.ContainerName, newName);
    }

    public static void SendSetRoleLock(ZDOID entityId, WardRole role, bool isDoor)
    {
        if (isDoor)
        {
            SendDoorRole(entityId, (int)role);
        }
        else
        {
            SendContainerRole(entityId, (int)role);
        }
    }

    public static void SendToggleFuelDepot(ZDOID containerId, bool isFuel)
    {
        SendContainerFuel(containerId, isFuel);
    }

    private static void RPC_SyncRegistry(long sender, ZPackage pkg)
    {
        WardMeBruh.Log.Debug($"[Network] Received ward registry sync package from peer {sender}");
        WardRegistryManager.Instance.Unpack(pkg);
    }

    private static void RPC_RequestSync(long sender)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        WardMeBruh.Log.Debug($"[Network] Received ward registry sync request from peer {sender}");
        ZPackage pkg = WardRegistryManager.Instance.Pack();
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcSyncRegistry, pkg);

        SendKnownPlayers(sender);
    }

    private static void RPC_UnregisterWard(long sender, ZPackage pkg)
    {
        if (pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        WardMeBruh.Log.Debug($"[Network] Received unregister ward request for Ward {wardId} from peer {sender}");
        WardRegistryManager.Instance.Unregister(wardId);

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            BroadcastRegistry();
        }
    }

    private static void RPC_TransferOwnership(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        long newOwnerId = pkg.ReadLong();
        string newOwnerName = pkg.ReadString();

        WardMeBruh.Log.Debug($"[Network] Received ownership transfer request for Ward {wardId} -> New Owner: {newOwnerName} ({newOwnerId}) from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Ownership transfer failed: Ward ZDO {wardId} not found");
            return;
        }

        long senderPlayerId = NetPeerUtility.GetPeerPlayerId(sender);
        long currentOwnerId = wardZdo.GetLong(ZDOVars.s_creator, 0L);

        bool isAuthorized = WardPermissions.IsAdmin(senderPlayerId) || (currentOwnerId != 0L && currentOwnerId == senderPlayerId);
        if (!isAuthorized)
        {
            WardMeBruh.Log.Debug($"[Network] Ownership transfer rejected: Sender {senderPlayerId} is not authorized for Ward {wardId}");
            return;
        }

        string currentOwnerName = wardZdo.GetString(ZDOVars.s_creatorName, "Previous Owner");

        wardZdo.Set(ZDOVars.s_creator, newOwnerId);
        wardZdo.Set(ZDOVars.s_creatorName, newOwnerName);

        WardRoleAssignments assignments = WardRoleManager.GetRoleAssignments(wardZdo);
        assignments.SetRole(currentOwnerId, currentOwnerName, (int)WardRole.Manager);
        assignments.Remove(newOwnerId);
        WardRoleManager.SaveRoleAssignments(wardZdo, assignments);

        WardMeBruh.Log.Debug($"[ZDO] Updated Ward {wardId} ownership: OldOwner {currentOwnerId} -> NewOwner {newOwnerId} ({newOwnerName})");

        WardRegistryManager.Instance.RegisterOrUpdate(wardZdo);
        BroadcastRegistry();
    }

    private static void RPC_SetEntityName(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID entityId = pkg.ReadZDOID();
        int keyHash = pkg.ReadInt();
        string newName = pkg.ReadString();

        WardMeBruh.Log.Debug($"[Network] Received entity rename: Entity {entityId}, KeyHash {keyHash}, NewName '{newName}' from sender {sender}");

        ZDO entityZdo = ZDOMan.instance.GetZDO(entityId);
        if (entityZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Entity rename failed: Entity ZDO {entityId} not found");
            return;
        }

        entityZdo.Set(keyHash, newName ?? string.Empty);
        WardMeBruh.Log.Debug($"[ZDO] Set entity {entityId} name key {keyHash} to '{newName}'");

        if (keyHash == WardZdoKeys.CustomName)
        {
            WardRegistryManager.Instance.RegisterOrUpdate(entityZdo);
            BroadcastRegistry();
        }
    }

    private static void RPC_SetDoorRole(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID doorId = pkg.ReadZDOID();
        int minRole = pkg.ReadInt();

        WardMeBruh.Log.Debug($"[Network] Received door role change: Door {doorId} -> MinRole {(WardRole)minRole} from sender {sender}");

        ZDO doorZdo = ZDOMan.instance.GetZDO(doorId);
        if (doorZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Door role change failed: Door ZDO {doorId} not found");
            return;
        }

        doorZdo.Set(WardZdoKeys.DoorMinRole, minRole);
        WardMeBruh.Log.Debug($"[ZDO] Set door {doorId} MinRole to {(WardRole)minRole}");
    }

    private static void RPC_SetContainerRole(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID containerId = pkg.ReadZDOID();
        int minRole = pkg.ReadInt();

        WardMeBruh.Log.Debug($"[Network] Received container role change: Container {containerId} -> MinRole {(WardRole)minRole} from sender {sender}");

        ZDO containerZdo = ZDOMan.instance.GetZDO(containerId);
        if (containerZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Container role change failed: Container ZDO {containerId} not found");
            return;
        }

        containerZdo.Set(WardZdoKeys.ContainerMinRole, minRole);
        WardMeBruh.Log.Debug($"[ZDO] Set container {containerId} MinRole to {(WardRole)minRole}");
    }

    private static void RPC_SetContainerFuel(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID containerId = pkg.ReadZDOID();
        bool isFuelDepot = pkg.ReadBool();

        WardMeBruh.Log.Debug($"[Network] Received container fuel depot setting: Container {containerId} -> IsFuelDepot {isFuelDepot} from sender {sender}");

        ZDO containerZdo = ZDOMan.instance.GetZDO(containerId);
        if (containerZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Container fuel setting failed: Container ZDO {containerId} not found");
            return;
        }

        containerZdo.Set(WardZdoKeys.ContainerIsFuel, isFuelDepot);
        WardMeBruh.Log.Debug($"[ZDO] Set container {containerId} IsFuelDepot to {isFuelDepot}");
    }

    private static void RPC_SetPortalRole(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID portalId = pkg.ReadZDOID();
        int minRole = pkg.ReadInt();

        WardMeBruh.Log.Debug($"[Network] Received portal role change: Portal {portalId} -> MinRole {(WardRole)minRole} from sender {sender}");

        ZDO portalZdo = ZDOMan.instance.GetZDO(portalId);
        if (portalZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Portal role change failed: Portal ZDO {portalId} not found");
            return;
        }

        portalZdo.Set(WardZdoKeys.PortalMinRole, minRole);
        ZDOMan.instance.SetDirtyPortals();
        WardMeBruh.Log.Debug($"[ZDO] Set portal {portalId} MinRole to {(WardRole)minRole}");
    }

    private static void RPC_SetWardRole(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        int minRole = pkg.ReadInt();

        WardMeBruh.Log.Debug($"[Network] Received ward role change: Ward {wardId} -> MinRole {(WardRole)minRole} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Ward role change failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(WardZdoKeys.WardMinRole, minRole);
        WardMeBruh.Log.Debug($"[ZDO] Set ward {wardId} MinRole to {(WardRole)minRole}");
    }

    private static void RPC_SetFeatureRole(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        int featureRoleKey = pkg.ReadInt();
        int minRole = pkg.ReadInt();

        WardMeBruh.Log.Debug($"[Network] Received feature role change: Ward {wardId}, Key {featureRoleKey} -> MinRole {(WardRole)minRole} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Feature role change failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(featureRoleKey, minRole);
        WardMeBruh.Log.Debug($"[ZDO] Set ward {wardId} Key {featureRoleKey} MinRole to {(WardRole)minRole}");
    }

    private static void RPC_SetWardBool(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null || pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        int keyHash = pkg.ReadInt();
        bool value = pkg.ReadBool();

        WardMeBruh.Log.Debug($"[Network] Received ward bool update: Ward {wardId}, Key {keyHash} -> {value} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Ward bool update failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(keyHash, value);
        WardMeBruh.Log.Debug($"[ZDO] Set ward {wardId} key {keyHash} to {value}");
    }

    private static void RPC_SetWardFloat(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null || pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        int keyHash = pkg.ReadInt();
        float value = pkg.ReadSingle();

        WardMeBruh.Log.Debug($"[Network] Received ward float update: Ward {wardId}, Key {keyHash} -> {value} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Ward float update failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(keyHash, value);
        WardMeBruh.Log.Debug($"[ZDO] Set ward {wardId} key {keyHash} to {value}");

        if (keyHash == WardZdoKeys.Radius)
        {
            WardRegistryManager.Instance.RegisterOrUpdate(wardZdo);
            BroadcastRegistry();
        }
    }

    private static void RPC_RegisterWard(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null || pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        WardMeBruh.Log.Debug($"[Server] Authoritative registration for Ward {wardId} from peer {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo != null)
        {
            WardRegistryManager.Instance.RegisterOrUpdate(wardZdo);
            BroadcastRegistry();
        }
    }

    public static void SendRoleDefinitions(ZDOID wardId, string serializedRoles)
    {
        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting role definitions: Ward {wardId}");
        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(serializedRoles ?? string.Empty);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetRoleDefinitions, pkg);

        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(WardZdoKeys.RoleDefinitions, serializedRoles ?? string.Empty);
            }
        }
    }

    private static void RPC_SetRoleDefinitions(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        if (pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        string serializedRoles = pkg.ReadString();

        WardMeBruh.Log.Debug($"[Network] Received role definitions update for Ward {wardId} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Role definitions update failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(WardZdoKeys.RoleDefinitions, serializedRoles);
        WardMeBruh.Log.Debug($"[ZDO] Updated Ward {wardId} RoleDefinitions");
    }

    public static void SendRoleAssignments(ZDOID wardId, string serializedAssignments)
    {
        if (ZRoutedRpc.instance == null)
            return;

        WardMeBruh.Log.Debug($"[Network] Transmitting role assignments: Ward {wardId}");
        ZPackage pkg = new();
        pkg.Write(wardId);
        pkg.Write(serializedAssignments ?? string.Empty);

        long target = ZNet.instance != null && ZNet.instance.IsServer() ? ZRoutedRpc.Everybody : ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(target, RpcSetRoleAssignments, pkg);

        if (ZDOMan.instance != null)
        {
            ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
            if (wardZdo != null)
            {
                wardZdo.Set(WardZdoKeys.RoleAssignments, serializedAssignments ?? string.Empty);
                WardRoleAssignments assignments = WardRoleAssignments.Deserialize(serializedAssignments ?? string.Empty);
                assignments.SyncToLegacy(out WardRoster managers, out WardRoster residents);
                wardZdo.Set(WardZdoKeys.RolesManagers, managers.Serialize());
                wardZdo.Set(WardZdoKeys.RolesResidents, residents.Serialize());
            }
        }
    }

    private static void RPC_SetRoleAssignments(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        if (pkg == null)
            return;

        ZDOID wardId = pkg.ReadZDOID();
        string serializedAssignments = pkg.ReadString();

        WardMeBruh.Log.Debug($"[Network] Received role assignments update for Ward {wardId} from sender {sender}");

        ZDO wardZdo = ZDOMan.instance.GetZDO(wardId);
        if (wardZdo == null)
        {
            WardMeBruh.Log.Debug($"[Network] Role assignments update failed: Ward ZDO {wardId} not found");
            return;
        }

        wardZdo.Set(WardZdoKeys.RoleAssignments, serializedAssignments);
        WardRoleAssignments assignments = WardRoleAssignments.Deserialize(serializedAssignments);
        assignments.SyncToLegacy(out WardRoster managers, out WardRoster residents);
        wardZdo.Set(WardZdoKeys.RolesManagers, managers.Serialize());
        wardZdo.Set(WardZdoKeys.RolesResidents, residents.Serialize());
        WardMeBruh.Log.Debug($"[ZDO] Updated Ward {wardId} RoleAssignments and legacy rosters");
    }

    public static void SendKnownPlayers(long targetPeerId)
    {
        if (ZRoutedRpc.instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        Dictionary<long, string> players = PlayerRegistryService.GetAllRegisteredPlayers();
        ZPackage pkg = new();
        pkg.Write(players.Count);
        foreach (KeyValuePair<long, string> kvp in players)
        {
            pkg.Write(kvp.Key);
            pkg.Write(kvp.Value ?? string.Empty);
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerId, RpcSyncAllPlayers, pkg);
    }

    public static void BroadcastPlayer(long playerId, string playerName)
    {
        if (ZRoutedRpc.instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        ZPackage pkg = new();
        pkg.Write(playerId);
        pkg.Write(playerName ?? string.Empty);

        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcSyncSinglePlayer, pkg);
    }

    private static void RPC_SyncAllPlayers(long sender, ZPackage pkg)
    {
        if (pkg == null)
            return;

        int count = pkg.ReadInt();
        for (int i = 0; i < count; i++)
        {
            long id = pkg.ReadLong();
            string name = pkg.ReadString();
            if (id != 0L && !string.IsNullOrEmpty(name))
            {
                PlayerRegistryService.RegisterAndPersistPlayer(id, name, false);
            }
        }
        WardMeBruh.Log.Info($"[Network] Received authoritative player registry sync from server ({count} players)");
    }

    private static void RPC_SyncSinglePlayer(long sender, ZPackage pkg)
    {
        if (pkg == null)
            return;

        long id = pkg.ReadLong();
        string name = pkg.ReadString();
        if (id != 0L && !string.IsNullOrEmpty(name))
        {
            PlayerRegistryService.RegisterAndPersistPlayer(id, name, false);
            WardMeBruh.Log.Info($"[Network] Received player update from server: '{name}' (ID: {id})");
        }
    }

    public static void RegisterPlayerWithServer(long playerId, string playerName)
    {
        if (ZRoutedRpc.instance == null || playerId == 0L || string.IsNullOrEmpty(playerName))
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            PlayerRegistryService.RegisterAndPersistPlayer(playerId, playerName, broadcast: true);
            return;
        }

        WardMeBruh.Log.Info($"[Network] Transmitting character registration to server: '{playerName}' (ID: {playerId})");
        ZPackage pkg = new();
        pkg.Write(playerId);
        pkg.Write(playerName);

        long serverPeer = ZRoutedRpc.instance.GetServerPeerID();
        ZRoutedRpc.instance.InvokeRoutedRPC(serverPeer, RpcRegisterPlayer, pkg);
    }

    private static void RPC_RegisterPlayer(long sender, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || pkg == null)
            return;

        long playerId = pkg.ReadLong();
        string playerName = pkg.ReadString();

        if (playerId != 0L && !string.IsNullOrEmpty(playerName))
        {
            WardMeBruh.Log.Info($"[Server] Authoritative registration for player '{playerName}' (ID: {playerId}) from peer {sender}");
            PlayerRegistryService.RegisterAndPersistPlayer(playerId, playerName, broadcast: true);
            SendKnownPlayers(sender);
        }
    }
}
