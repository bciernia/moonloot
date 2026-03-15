using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFlagsManager : Singleton<DialogueFlagsManager>
{
    [SerializeField] private List<string> _stringFlags = new();

    public void AddFlagToHashSet(string flagName)
    {
        if(!_stringFlags.Contains(flagName))
            Instance._stringFlags.Add(flagName);
    }

    public bool IsFlagInHashSet(string flagName)
    {
        return Instance._stringFlags.Contains(flagName);
    }
}