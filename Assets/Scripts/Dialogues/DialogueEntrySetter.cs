using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueEntrySetter : MonoBehaviour
{
    public string NpcName { get; private set; }

    private void Awake()
    {
        NpcName = gameObject.name;
    }

    public void UpdateDialogueEntry([CanBeNull] string npcName, string dialogueEntryId)
    {
        DialogueEntryManager.Instance.UpdateNpcDialogueEntry(npcName ?? NpcName, dialogueEntryId);
    }

    public string GetEntryId() => DialogueEntryManager.Instance.GetDialogueEntryIdByNpcName(NpcName);
}
