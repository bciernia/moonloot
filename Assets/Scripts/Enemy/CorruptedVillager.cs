using System;
using UnityEngine;

public class CorruptedVillager : MonoBehaviour
{
    public static Action OnCorruptedVillagerKilled;

    private bool _destroyed;

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;

        OnCorruptedVillagerKilled?.Invoke();
    }
}