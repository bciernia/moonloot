using System.Collections.Generic;

public class DialogueEntryManager : Singleton<DialogueEntryManager>
{
    private readonly Dictionary<string, string> NpcDialogueEntryDictionary = new();

    public void UpdateNpcDialogueEntry(string npcName, string dialogueEntryId)
    {
        NpcDialogueEntryDictionary[npcName] = dialogueEntryId;
    }

    public string GetDialogueEntryIdByNpcName(string npcName) => NpcDialogueEntryDictionary.GetValueOrDefault(npcName, "0");
}
