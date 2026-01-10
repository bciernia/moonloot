using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcDialogueFlagCondition : MonoBehaviour
{
    [SerializeField] private List<string> flags;
    [SerializeField] private bool useAllFlags;

    private void Start()
    {
        if (flags == null || flags.Count == 0)
            return;

        if (ShowNpc())
            return;            
            
        Destroy(gameObject);
    }

    private bool ShowNpc() => useAllFlags ? AllFlagsInHashSet() : AnyFlagInHashSet();

    private bool AnyFlagInHashSet() => flags.Any(flag => DialogueFlagsManager.Instance.IsFlagInHashSet(flag));

    private bool AllFlagsInHashSet() => flags.All(flag => DialogueFlagsManager.Instance.IsFlagInHashSet(flag));
}