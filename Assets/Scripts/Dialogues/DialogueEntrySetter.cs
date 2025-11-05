using UnityEngine;

public class DialogueEntrySetter : MonoBehaviour
{
    public void UpdateDialogueEntry(string dialogueEntryId)
    {
        DialogueEntryManager.Instance.UpdateNpcDialogueEntry(dialogueEntryId, gameObject.name);
    }

    public string GetEntryId() => DialogueEntryManager.Instance.GetDialogueEntryIdByNpcName(gameObject.name);
}
