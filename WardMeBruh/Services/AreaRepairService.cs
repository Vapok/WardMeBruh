using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;

namespace WardMeBruh.Services;

public class AreaRepairService : MonoBehaviour
{
    private static AreaRepairService _instance;
    private const float CheckIntervalSeconds = 3.0f;
    private const int MaxRepairsPerWardPerTick = 10;

    public static AreaRepairService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject serviceObject = new GameObject("WardMeBruh_AreaRepairService");
                DontDestroyOnLoad(serviceObject);
                _instance = serviceObject.AddComponent<AreaRepairService>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        StartCoroutine(RepairLoopCoroutine());
    }

    private IEnumerator RepairLoopCoroutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(CheckIntervalSeconds);

        while (true)
        {
            yield return waitInterval;

            if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
                continue;

            if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
                continue;

            List<PrivateArea> activeRepairWards = null;
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea ward = PrivateArea.m_allAreas[i];
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
                    continue;

                if (!ward.IsEnabled())
                    continue;

                ZDO zdo = ward.m_nview.GetZDO();
                if (zdo == null)
                    continue;

                ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
                if (effectiveZdo == null || !WardQueryService.HasFeatureEnabled(effectiveZdo, WardZdoKeys.RepairEnabled))
                    continue;

                bool isServer = ZNet.instance != null && ZNet.instance.IsServer();
                bool isWardOwner = ward.m_nview.IsOwner();
                bool isPlayerInside = Player.m_localPlayer != null && WardQueryService.IsInsideWard(ward, Player.m_localPlayer.transform.position);

                if (!isServer && !isWardOwner && !isPlayerInside)
                    continue;

                activeRepairWards ??= new List<PrivateArea>();
                activeRepairWards.Add(ward);
            }

            if (activeRepairWards == null || activeRepairWards.Count == 0)
                continue;

            List<WearNTear> allPieces = WearNTear.GetAllInstances();
            if (allPieces == null || allPieces.Count == 0)
                continue;

            for (int wardIndex = 0; wardIndex < activeRepairWards.Count; wardIndex++)
            {
                PrivateArea ward = activeRepairWards[wardIndex];
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.IsEnabled())
                    continue;

                ZDO wardZdo = ward.m_nview.GetZDO();
                if (wardZdo == null)
                    continue;

                ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
                if (effectiveZdo == null || !WardQueryService.HasFeatureEnabled(effectiveZdo, WardZdoKeys.RepairEnabled))
                    continue;

                float radius = WardQueryService.GetWardRadius(ward);
                float thresholdPercent = effectiveZdo.GetFloat(
                    WardZdoKeys.RepairThreshold, 
                    (float)ConfigRegistry.AutoRepairThresholdDefault.Value);

                if (thresholdPercent > 1.0f)
                {
                    thresholdPercent /= 100f;
                }

                Vector3 wardPos = ward.transform.position;
                int repairedCount = 0;

                for (int pieceIndex = 0; pieceIndex < allPieces.Count; pieceIndex++)
                {
                    WearNTear piece = allPieces[pieceIndex];
                    if (piece == null || piece.m_nview == null || !piece.m_nview.IsValid())
                        continue;

                    if (Utils.DistanceXZ(wardPos, piece.transform.position) > radius)
                        continue;

                    float healthPercent = piece.GetHealthPercentage();
                    if (healthPercent < thresholdPercent && healthPercent < 1.0f)
                    {
                        if (piece.Repair())
                        {
                            repairedCount++;
                            Piece pieceComp = piece.GetComponent<Piece>();
                            if (pieceComp != null && pieceComp.m_placeEffect != null)
                            {
                                pieceComp.m_placeEffect.Create(piece.transform.position, piece.transform.rotation, null, 1f, -1, ZDOID.None);
                            }

                            if (repairedCount >= MaxRepairsPerWardPerTick)
                                break;
                        }
                    }
                }

                if (repairedCount > 0)
                {
                    string wardName = effectiveZdo.GetString(WardZdoKeys.CustomName, "Ward");
                    WardMeBruh.Log.Info($"[AutoRepair] Ward '{wardName}' ({effectiveZdo.m_uid}) auto-repaired {repairedCount} damaged piece(s)");
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
