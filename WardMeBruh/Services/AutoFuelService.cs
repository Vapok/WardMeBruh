using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Patches;

namespace WardMeBruh.Services;

public class AutoFuelService : MonoBehaviour
{
    private static AutoFuelService _instance;
    private const float CheckIntervalSeconds = 10.0f;

    public static AutoFuelService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject serviceObject = new GameObject("WardMeBruh_AutoFuelService");
                DontDestroyOnLoad(serviceObject);
                _instance = serviceObject.AddComponent<AutoFuelService>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        StartCoroutine(FuelingLoopCoroutine());
    }

    private IEnumerator FuelingLoopCoroutine()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(CheckIntervalSeconds);

        while (true)
        {
            yield return waitInterval;

            if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
                continue;

            if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
                continue;

            List<PrivateArea> activeFuelWards = null;
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea ward = PrivateArea.m_allAreas[i];
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.m_nview.IsOwner())
                    continue;

                if (!ward.IsEnabled())
                    continue;

                ZDO zdo = ward.m_nview.GetZDO();
                if (zdo == null)
                    continue;

                ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
                if (effectiveZdo != null && WardQueryService.HasFeatureEnabled(effectiveZdo, WardZdoKeys.AutoFuelEnabled))
                {
                    activeFuelWards ??= new List<PrivateArea>();
                    activeFuelWards.Add(ward);
                }
            }

            if (activeFuelWards == null || activeFuelWards.Count == 0)
                continue;

            List<Container> allContainers = Patch_Container.AllContainers;
            List<Fireplace> allFireplaces = Patch_Fireplace.AllFireplaces;
            List<Smelter> allSmelters = Patch_Smelter.AllSmelters;
            List<CookingStation> allCookingStations = Patch_CookingStation.AllCookingStations;

            allContainers.RemoveAll(c => c == null);
            allFireplaces.RemoveAll(f => f == null);
            allSmelters.RemoveAll(s => s == null);
            allCookingStations.RemoveAll(cs => cs == null);

            for (int wardIndex = 0; wardIndex < activeFuelWards.Count; wardIndex++)
            {
                PrivateArea ward = activeFuelWards[wardIndex];
                if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid() || !ward.m_nview.IsOwner() || !ward.IsEnabled())
                    continue;

                ZDO wardZdo = ward.m_nview.GetZDO();
                if (wardZdo == null)
                    continue;

                ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
                if (effectiveZdo == null || !WardQueryService.HasFeatureEnabled(effectiveZdo, WardZdoKeys.AutoFuelEnabled))
                    continue;

                WardType wardType = (WardType)wardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                bool isSubWard = (wardType == WardType.SubWard);
                float radius = WardQueryService.GetWardRadius(ward);
                Vector3 wardPos = ward.transform.position;

                List<Container> fuelContainers = new();
                for (int cIndex = 0; cIndex < allContainers.Count; cIndex++)
                {
                    Container container = allContainers[cIndex];
                    if (container == null || container.m_nview == null || !container.m_nview.IsValid())
                        continue;

                    if (!container.m_nview.GetZDO().GetBool(WardZdoKeys.ContainerIsFuel, false))
                        continue;

                    Vector3 containerPos = container.transform.position;

                    if (Utils.DistanceXZ(wardPos, containerPos) > radius)
                        continue;

                    PrivateArea coveringSubWard = WardQueryService.GetActiveSubWardCovering(containerPos);
                    if (isSubWard)
                    {
                        if (coveringSubWard != ward)
                            continue;
                    }
                    else
                    {
                        if (coveringSubWard != null)
                            continue;
                    }

                    fuelContainers.Add(container);
                }

                if (fuelContainers.Count == 0)
                    continue;

                for (int fIndex = 0; fIndex < allFireplaces.Count; fIndex++)
                {
                    Fireplace fireplace = allFireplaces[fIndex];
                    if (fireplace == null || fireplace.m_nview == null || !fireplace.m_nview.IsValid())
                        continue;

                    Vector3 fireplacePos = fireplace.transform.position;

                    if (Utils.DistanceXZ(wardPos, fireplacePos) > radius)
                        continue;

                    PrivateArea coveringSubWard = WardQueryService.GetActiveSubWardCovering(fireplacePos);
                    if (isSubWard)
                    {
                        if (coveringSubWard != ward)
                            continue;
                    }
                    else
                    {
                        if (coveringSubWard != null)
                            continue;
                    }

                    if (!fireplace.m_canRefill || fireplace.m_infiniteFuel || fireplace.m_fuelItem == null)
                        continue;

                    float currentFuel = fireplace.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
                    if (currentFuel >= fireplace.m_maxFuel - 1f)
                        continue;

                    string fuelName = fireplace.m_fuelItem.m_itemData.m_shared.m_name;
                    for (int fcIndex = 0; fcIndex < fuelContainers.Count; fcIndex++)
                    {
                        Container fuelContainer = fuelContainers[fcIndex];
                        if (fuelContainer == null)
                            continue;

                        Inventory inv = fuelContainer.GetInventory();
                        if (inv != null && inv.HaveItem(fuelName))
                        {
                            inv.RemoveItem(fuelName, 1);
                            fireplace.m_nview.InvokeRPC("RPC_AddFuel");
                            string containerName = fuelContainer.m_nview.GetZDO().GetString(WardZdoKeys.ContainerName, "Chest");
                            WardMeBruh.Log.Debug($"[AutoFuel] Fueled Fireplace ({fireplace.m_nview.GetZDO().m_uid}) with 1x '{fuelName}' from Container '{containerName}' ({fuelContainer.m_nview.GetZDO().m_uid}) in Ward {ward.m_nview.GetZDO().m_uid}");
                            break;
                        }
                    }
                }

                for (int sIndex = 0; sIndex < allSmelters.Count; sIndex++)
                {
                    Smelter smelter = allSmelters[sIndex];
                    if (smelter == null || smelter.m_nview == null || !smelter.m_nview.IsValid())
                        continue;

                    Vector3 smelterPos = smelter.transform.position;

                    if (Utils.DistanceXZ(wardPos, smelterPos) > radius)
                        continue;

                    PrivateArea coveringSubWard = WardQueryService.GetActiveSubWardCovering(smelterPos);
                    if (isSubWard)
                    {
                        if (coveringSubWard != ward)
                            continue;
                    }
                    else
                    {
                        if (coveringSubWard != null)
                            continue;
                    }

                    if (smelter.m_maxFuel <= 0 || smelter.m_fuelItem == null)
                        continue;

                    float currentFuel = smelter.GetFuel();
                    if (currentFuel >= smelter.m_maxFuel - 1f)
                        continue;

                    string fuelName = smelter.m_fuelItem.m_itemData.m_shared.m_name;
                    for (int fcIndex = 0; fcIndex < fuelContainers.Count; fcIndex++)
                    {
                        Container fuelContainer = fuelContainers[fcIndex];
                        if (fuelContainer == null)
                            continue;

                        Inventory inv = fuelContainer.GetInventory();
                        if (inv != null && inv.HaveItem(fuelName))
                        {
                            inv.RemoveItem(fuelName, 1);
                            smelter.m_nview.InvokeRPC("RPC_AddFuel");
                            string containerName = fuelContainer.m_nview.GetZDO().GetString(WardZdoKeys.ContainerName, "Chest");
                            WardMeBruh.Log.Debug($"[AutoFuel] Fueled Smelter ({smelter.m_nview.GetZDO().m_uid}) with 1x '{fuelName}' from Container '{containerName}' ({fuelContainer.m_nview.GetZDO().m_uid}) in Ward {ward.m_nview.GetZDO().m_uid}");
                            break;
                        }
                    }
                }

                for (int csIndex = 0; csIndex < allCookingStations.Count; csIndex++)
                {
                    CookingStation station = allCookingStations[csIndex];
                    if (station == null || station.m_nview == null || !station.m_nview.IsValid())
                        continue;

                    Vector3 stationPos = station.transform.position;

                    if (Utils.DistanceXZ(wardPos, stationPos) > radius)
                        continue;

                    PrivateArea coveringSubWard = WardQueryService.GetActiveSubWardCovering(stationPos);
                    if (isSubWard)
                    {
                        if (coveringSubWard != ward)
                            continue;
                    }
                    else
                    {
                        if (coveringSubWard != null)
                            continue;
                    }

                    if (!station.m_useFuel || station.m_maxFuel <= 0 || station.m_fuelItem == null)
                        continue;

                    float currentFuel = station.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, 0f);
                    if (currentFuel >= station.m_maxFuel - 1f)
                        continue;

                    string fuelName = station.m_fuelItem.m_itemData.m_shared.m_name;
                    for (int fcIndex = 0; fcIndex < fuelContainers.Count; fcIndex++)
                    {
                        Container fuelContainer = fuelContainers[fcIndex];
                        if (fuelContainer == null)
                            continue;

                        Inventory inv = fuelContainer.GetInventory();
                        if (inv != null && inv.HaveItem(fuelName))
                        {
                            inv.RemoveItem(fuelName, 1);
                            station.m_nview.InvokeRPC("RPC_AddFuel");
                            string containerName = fuelContainer.m_nview.GetZDO().GetString(WardZdoKeys.ContainerName, "Chest");
                            WardMeBruh.Log.Debug($"[AutoFuel] Fueled CookingStation/Oven ({station.m_nview.GetZDO().m_uid}) with 1x '{fuelName}' from Container '{containerName}' ({fuelContainer.m_nview.GetZDO().m_uid}) in Ward {ward.m_nview.GetZDO().m_uid}");
                            break;
                        }
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
