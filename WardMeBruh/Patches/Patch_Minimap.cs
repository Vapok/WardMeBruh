using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Minimap))]
internal static class Patch_Minimap
{
    private static readonly Dictionary<ZDOID, Minimap.PinData> s_wardPins = new();

    [HarmonyPatch(nameof(Minimap.UpdatePins))]
    [HarmonyPostfix]
    private static void Postfix_UpdatePins(Minimap __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
        {
            if (s_wardPins.Count > 0)
            {
                foreach (KeyValuePair<ZDOID, Minimap.PinData> kvp in s_wardPins)
                {
                    __instance.RemovePin(kvp.Value);
                }
                s_wardPins.Clear();
            }
            return;
        }

        if (Player.m_localPlayer == null)
            return;

        long playerId = Player.m_localPlayer.GetPlayerID();
        List<KnownWard> wards = WardRegistryManager.Instance.GetPlayerWards(playerId);

        HashSet<ZDOID> activeIds = new();

        for (int i = 0; i < wards.Count; i++)
        {
            KnownWard ward = wards[i];
            activeIds.Add(ward.Id);

            if (!s_wardPins.TryGetValue(ward.Id, out Minimap.PinData pin))
            {
                string label = string.IsNullOrEmpty(ward.Name) ? "$wmb_ward".Localize() : ward.Name;
                pin = __instance.AddPin(ward.Position, Minimap.PinType.Icon3, label, false, false);
                s_wardPins[ward.Id] = pin;
            }
            else
            {
                pin.m_pos = ward.Position;
                pin.m_name = string.IsNullOrEmpty(ward.Name) ? "$wmb_ward".Localize() : ward.Name;
            }
        }

        List<ZDOID> toRemove = new();
        foreach (KeyValuePair<ZDOID, Minimap.PinData> kvp in s_wardPins)
        {
            if (!activeIds.Contains(kvp.Key))
            {
                __instance.RemovePin(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            s_wardPins.Remove(toRemove[i]);
        }
    }

    public static bool TryFastTravel(KnownWard targetWard)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return false;

        if (Player.m_localPlayer == null || targetWard == null)
            return false;

        Player player = Player.m_localPlayer;

        if (!player.IsTeleportable(ConfigRegistry.AllowOreFastTravel.Value))
        {
            player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
            return false;
        }

        Vector3 targetPos = targetWard.Position + Vector3.up * 1f;
        player.TeleportTo(targetPos, Quaternion.identity, true);
        return true;
    }
}
