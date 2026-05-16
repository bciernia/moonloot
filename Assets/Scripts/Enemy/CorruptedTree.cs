using System;
using UnityEngine;

public class CorruptedTree : MonoBehaviour
{
    public static Action OnTreeDestroyed;

    private bool _destroyed;

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;

        OnTreeDestroyed?.Invoke();
    }
}