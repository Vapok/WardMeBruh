using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;

namespace WardMeBruh.Services;

public class AreaRepairService : MonoBehaviour
{
    private static AreaRepairService _instance;
    private const float CheckIntervalSeconds = 5.0f;
    private const int MaxRepairsPerWardPerTick = 5;

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
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.m_nview.IsOwner())
                    continue;

                ZDO zdo = ward.m_nview.GetZDO();
                if (zdo != null && zdo.GetBool(WardZdoKeys.RepairEnabled, false))
                {
                    activeRepairWards ??= new List<PrivateArea>();
                    activeRepairWards.Add(ward);
                }
            }

            if (activeRepairWards == null || activeRepairWards.Count == 0)
                continue;

            List<WearNTear> allPieces = WearNTear.GetAllInstances();
            if (allPieces == null || allPieces.Count == 0)
                continue;

            for (int wardIndex = 0; wardIndex < activeRepairWards.Count; wardIndex++)
            {
                PrivateArea ward = activeRepairWards[wardIndex];
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.m_nview.IsOwner())
                    continue;

                ZDO wardZdo = ward.m_nview.GetZDO();
                if (wardZdo == null)
                    continue;

                float radius = WardQueryService.GetWardRadius(ward);
                float thresholdPercent = wardZdo.GetFloat(
                    WardZdoKeys.RepairThreshold, 
                    (float)ConfigRegistry.AutoRepairThresholdDefault.Value) / 100f;

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
                            if (repairedCount >= MaxRepairsPerWardPerTick)
                                break;
                        }
                    }
                }

                if (repairedCount > 0)
                {
                    string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");
                    WardMeBruh.Log.Debug($"[AutoRepair] Ward '{wardName}' ({wardZdo.m_uid}) auto-repaired {repairedCount} damaged piece(s)");
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
