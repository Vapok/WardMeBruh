using HarmonyLib;
using UnityEngine;
using WardMeBruh.Components;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.CheckProjectile))]
internal static class Patch_ShieldGenerator
{
    private static void Postfix(Projectile projectile)
    {
        if (projectile == null || projectile.gameObject == null)
            return;

        for (int i = 0; i < WardForcefieldDome.Instances.Count; i++)
        {
            WardForcefieldDome dome = WardForcefieldDome.Instances[i];
            if (dome == null || !dome.IsActive)
                continue;

            float radius = dome.CurrentRadius;
            Vector3 center = dome.transform.position;

            float distStart = Vector3.Distance(center, projectile.m_startPoint);
            float distCurrent = Vector3.Distance(center, projectile.transform.position);

            if (distStart > radius && distCurrent < radius)
            {
                if (projectile.m_owner is Player player && WardPermissions.HasWardAccess(dome.Area, player.GetPlayerID()))
                {
                    continue;
                }

                dome.TriggerHit();
                ZNetScene.instance.Destroy(projectile.gameObject);
                break;
            }
        }
    }
}
