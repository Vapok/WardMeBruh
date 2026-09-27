using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Components;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public class WardRepulsionService : MonoBehaviour
{
    private static WardRepulsionService _instance;
    private const float CheckIntervalSeconds = 0.25f;
    private readonly Dictionary<int, float> _lastDefenseLogTimes = new();

    public static WardRepulsionService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject serviceObject = new GameObject("WardMeBruh_WardRepulsionService");
                DontDestroyOnLoad(serviceObject);
                _instance = serviceObject.AddComponent<WardRepulsionService>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        StartCoroutine(RepulsionLoopCoroutine());
    }

    private IEnumerator RepulsionLoopCoroutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(CheckIntervalSeconds);

        while (true)
        {
            yield return waitInterval;

            if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
                continue;

            if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
                continue;

            List<Character> allCharacters = Character.GetAllCharacters();
            if (allCharacters == null || allCharacters.Count == 0)
                continue;

            for (int wardIndex = 0; wardIndex < PrivateArea.m_allAreas.Count; wardIndex++)
            {
                PrivateArea ward = PrivateArea.m_allAreas[wardIndex];
                if (ward == null || !ward.IsEnabled() || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.m_nview.IsOwner())
                    continue;

                ZDO wardZdo = ward.m_nview.GetZDO();
                float pushbackForce = wardZdo.GetFloat(WardZdoKeys.PushbackForce, 0f);
                float shockCreatureDmg = wardZdo.GetFloat(WardZdoKeys.ShockCreatureDmg, 0f);
                float shockPlayerDmg = wardZdo.GetFloat(WardZdoKeys.ShockPlayerDmg, 0f);

                if (pushbackForce <= 0f && shockCreatureDmg <= 0f && shockPlayerDmg <= 0f)
                    continue;

                float radius = WardQueryService.GetWardRadius(ward);
                Vector3 wardPos = ward.transform.position;

                for (int charIndex = 0; charIndex < allCharacters.Count; charIndex++)
                {
                    Character character = allCharacters[charIndex];
                    if (character == null || character.IsDead() || WardQueryService.IsTamedOrTaming(character))
                        continue;

                    float dist = Utils.DistanceXZ(wardPos, character.transform.position);
                    if (dist > radius)
                        continue;

                    if (character is Player player)
                    {
                        if (WardPermissions.HasWardAccess(ward, player.GetPlayerID()))
                            continue;

                        int targetId = character.GetInstanceID();
                        float now = Time.time;
                        if (!_lastDefenseLogTimes.TryGetValue(targetId, out float lastLog) || now - lastLog >= 2.0f)
                        {
                            string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");
                            WardMeBruh.Log.Debug($"[Ward Defense] Ward '{wardName}' ({wardZdo.m_uid}) repelling intruder '{player.GetPlayerName()}' ({player.GetPlayerID()}) - Pushback: {pushbackForce:F0}, Shock: {shockPlayerDmg:F0}");
                            _lastDefenseLogTimes[targetId] = now;
                        }

                        if (pushbackForce > 0f)
                        {
                            Vector3 pushDir = player.transform.position - wardPos;
                            pushDir.y = 0.2f;
                            pushDir.Normalize();
                            player.ApplyPushback(pushDir, pushbackForce);
                        }

                        if (shockPlayerDmg > 0f)
                        {
                            HitData hit = new HitData();
                            hit.m_damage.m_lightning = shockPlayerDmg;
                            hit.m_point = player.transform.position;
                            player.Damage(hit);
                        }

                        WardForcefieldDome dome = ward.GetComponent<WardForcefieldDome>();
                        if (dome != null)
                        {
                            dome.TriggerHit();
                        }
                    }
                    else
                    {
                        int targetId = character.GetInstanceID();
                        float now = Time.time;
                        if (!_lastDefenseLogTimes.TryGetValue(targetId, out float lastLog) || now - lastLog >= 2.0f)
                        {
                            _lastDefenseLogTimes[targetId] = now;
                            string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");
                            WardMeBruh.Log.Debug($"[Ward Defense] Ward '{wardName}' ({wardZdo.m_uid}) repelling creature '{character.name}' - Pushback: {pushbackForce:F0}, Shock: {shockCreatureDmg:F0}");
                        }

                        if (pushbackForce > 0f)
                        {
                            Vector3 pushDir = character.transform.position - wardPos;
                            pushDir.y = 0.2f;
                            pushDir.Normalize();
                            character.ApplyPushback(pushDir, pushbackForce);
                        }

                        if (shockCreatureDmg > 0f)
                        {
                            HitData hit = new HitData();
                            hit.m_damage.m_lightning = shockCreatureDmg;
                            hit.m_point = character.transform.position;
                            character.Damage(hit);
                        }

                        WardForcefieldDome dome = ward.GetComponent<WardForcefieldDome>();
                        if (dome != null)
                        {
                            dome.TriggerHit();
                        }
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        _lastDefenseLogTimes.Clear();
    }
}
