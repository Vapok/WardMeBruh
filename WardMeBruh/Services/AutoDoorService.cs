using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;

namespace WardMeBruh.Services;

public class AutoDoorService : MonoBehaviour
{
    private static AutoDoorService _instance;
    private readonly Dictionary<Door, Coroutine> _activeCoroutines = new();
    private readonly WaitForSeconds _postponeWait = new(1.5f);

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

        if (door.m_nview == null || !door.m_nview.IsValid())
            return;

        CancelDoorClose(door);

        int currentState = door.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
        if (currentState == 0)
            return;

        float delay = coveringWard.m_nview.GetZDO().GetFloat(
            WardZdoKeys.AutoCloseDelay, 
            (float)ConfigRegistry.AutoCloseDelayDefault.Value);

        if (delay <= 0f)
            delay = 5f;

        string doorName = door.m_nview.GetZDO().GetString(WardZdoKeys.DoorName, "Door");
        WardMeBruh.Log.Debug($"[AutoDoor] Queuing auto-close for Door '{doorName}' ({door.m_nview.GetZDO().m_uid}) in {delay:F1}s");
        Coroutine coroutine = StartCoroutine(CloseDoorCoroutine(door, delay));
        _activeCoroutines[door] = coroutine;
    }

    public void CancelDoorClose(Door door)
    {
        if (door == null)
            return;

        if (_activeCoroutines.TryGetValue(door, out Coroutine existing))
        {
            if (existing != null)
            {
                StopCoroutine(existing);
            }
            _activeCoroutines.Remove(door);
        }
    }

    private IEnumerator CloseDoorCoroutine(Door door, float delay)
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            if (door == null || door.m_nview == null || !door.m_nview.IsValid())
            {
                if (door != null)
                {
                    _activeCoroutines.Remove(door);
                }
                yield break;
            }

            int currentState = door.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
            if (currentState == 0)
            {
                _activeCoroutines.Remove(door);
                yield break;
            }

            if (IsPlayerNearDoor(door, 3.5f))
            {
                yield return _postponeWait;
                continue;
            }

            _activeCoroutines.Remove(door);

            string doorName = door.m_nview.GetZDO().GetString(WardZdoKeys.DoorName, "Door");
            WardMeBruh.Log.Debug($"[AutoDoor] Auto-closing open Door '{doorName}' ({door.m_nview.GetZDO().m_uid})");
            door.m_nview.InvokeRPC("UseDoor", false);
            yield break;
        }
    }

    private bool IsPlayerNearDoor(Door door, float radius)
    {
        if (door == null)
            return false;

        Vector3 doorPos = door.transform.position;
        float radiusSq = radius * radius;

        if (Player.m_localPlayer != null && !Player.m_localPlayer.IsDead())
        {
            if ((Player.m_localPlayer.transform.position - doorPos).sqrMagnitude <= radiusSq)
                return true;
        }

        List<Player> allPlayers = Player.GetAllPlayers();
        if (allPlayers != null)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                Player player = allPlayers[i];
                if (player != null && !player.IsDead())
                {
                    if ((player.transform.position - doorPos).sqrMagnitude <= radiusSq)
                        return true;
                }
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        _activeCoroutines.Clear();
    }
}
