using UnityEngine;

/// <summary>
/// Used in dialogue nodes. The dialogue knows whether it should be shown or not
/// </summary>
public class DialogueFlags : MonoBehaviour
{
    public void Add(string flag)
    {
        DialogueFlagsManager.Instance.AddFlagToHashSet(flag);
    }

    public bool IsFlagAdded(string flag)
    {
        return DialogueFlagsManager.Instance.IsFlagInHashSet(flag);
    }
}
