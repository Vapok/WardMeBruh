using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
internal static class Patch_SkillsDeath
{
    private static readonly AccessTools.FieldRef<Skills, Player> s_playerRef = 
        AccessTools.FieldRefAccess<Skills, Player>("m_player");

    [HarmonyPrefix]
    private static bool Prefix(Skills __instance)
    {
        Player player = s_playerRef(__instance);
        if (player == null)
        {
            player = __instance.GetComponent<Player>();
        }

        if (player == null)
            return true;

        if (WardQueryService.HasFeatureEnabled(player.transform.position, WardZdoKeys.DeathSkillShield, player.GetPlayerID()))
        {
            WardMeBruh.Log.Debug($"[Skill Protection] Ward shielded player '{player.GetPlayerName()}' ({player.GetPlayerID()}) skills from death loss at {player.transform.position}");
            player.Message(MessageHud.MessageType.Center, "$wmb_msg_death_skill_shield".Localize());
            return false;
        }

        return true;
    }
}
