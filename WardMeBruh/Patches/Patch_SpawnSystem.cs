using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(SpawnSystem))]
internal static class Patch_SpawnSystem
{
    [HarmonyPatch("IsSpawnPointGood")]
    [HarmonyPrefix]
    private static bool Prefix_IsSpawnPointGood(SpawnSystem.SpawnData spawn, ref Vector3 spawnPoint, ref bool __result)
    {
        if (WardQueryService.HasFeatureEnabled(spawnPoint, WardZdoKeys.SpawnSuppress))
        {
            string prefabName = (spawn != null && spawn.m_prefab != null) ? spawn.m_prefab.name : "Creature";
            WardMeBruh.Log.Debug($"[Spawn] Suppressed spawn of '{prefabName}' at {spawnPoint} by active ward");
            __result = false;
            return false;
        }

        return true;
    }
}
