using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(SpawnSystem))]
internal static class Patch_SpawnSystem
{
    [HarmonyPatch("IsSpawnPointGood", new[] { typeof(SpawnSystem.SpawnData), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Ref })]
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

    [HarmonyPatch("Spawn", typeof(SpawnSystem.SpawnData), typeof(Vector3), typeof(bool))]
    [HarmonyPrefix]
    private static bool Prefix_Spawn(SpawnSystem.SpawnData critter, Vector3 spawnPoint)
    {
        if (WardQueryService.HasFeatureEnabled(spawnPoint, WardZdoKeys.SpawnSuppress))
        {
            string prefabName = (critter != null && critter.m_prefab != null) ? critter.m_prefab.name : "Creature";
            WardMeBruh.Log.Debug($"[Spawn] Suppressed Spawn of '{prefabName}' at {spawnPoint} by active ward");
            return false;
        }

        return true;
    }
}
