using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;

namespace WardMeBruh.Services;

public class AutoDoorService : MonoBehaviour
{
    private static AutoDoorService _instance;
    private readonly HashSet<Door> _pendingDoors = new();

    public static AutoDoorService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject serviceObject = new GameObject("WardMeBruh_AutoDoorService");
                DontDestroyOnLoad(serviceObject);
                _instance = serviceObject.AddComponent<AutoDoorService>();
            }
            return _instance;
        }
    }

    public void QueueDoorClose(Door door, PrivateArea coveringWard)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (door == null || coveringWard == null || coveringWard.m_nview == null || !coveringWard.m_nview.IsValid())
            return;

        if (_pendingDoors.Contains(door))
            return;

        float delay = coveringWard.m_nview.GetZDO().GetFloat(
            WardZdoKeys.AutoCloseDelay, 
            (float)ConfigRegistry.AutoCloseDelayDefault.Value);

        if (delay <= 0f)
            delay = 5f;

        _pendingDoors.Add(door);
        string doorName = door.m_nview.GetZDO().GetString(WardZdoKeys.DoorName, "Door");
        WardMeBruh.Log.Debug($"[AutoDoor] Queuing auto-close for Door '{doorName}' ({door.m_nview.GetZDO().m_uid}) in {delay:F1}s");
        StartCoroutine(CloseDoorCoroutine(door, delay));
    }

    private IEnumerator CloseDoorCoroutine(Door door, float delay)
    {
        yield return new WaitForSeconds(delay);

        _pendingDoors.Remove(door);

        if (door == null || door.m_nview == null || !door.m_nview.IsValid())
            yield break;

        int currentState = door.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
        if (currentState != 0)
        {
            string doorName = door.m_nview.GetZDO().GetString(WardZdoKeys.DoorName, "Door");
            WardMeBruh.Log.Debug($"[AutoDoor] Auto-closing open Door '{doorName}' ({door.m_nview.GetZDO().m_uid})");
            door.m_nview.InvokeRPC("UseDoor", false);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        _pendingDoors.Clear();
    }
}
