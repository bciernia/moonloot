using System;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueEntrySetter : MonoBehaviour
{
    [SerializeField] private string DialogueEntryId;

    private string EntryId { get; set; }
    
    public void SetEntryId(string entryId)
    {
        DialogueEntryId = entryId;
        EntryId = DialogueEntryId;
    }

    public string GetEntryId() => EntryId ?? DialogueEntryId;
}
