using System.Collections.Generic;

public class DialogueEntryManager : Singleton<DialogueEntryManager>
{
    //TODO Uwzględnić przy zapisie
    private readonly Dictionary<string, string> NpcDialogueEntryDictionary = new();

    public void UpdateNpcDialogueEntry(string dialogueEntryId, string gameObjectName)
    {
        NpcDialogueEntryDictionary[gameObjectName] = dialogueEntryId;
    }

    public string GetDialogueEntryIdByNpcName(string gameObjectName) => NpcDialogueEntryDictionary.GetValueOrDefault(gameObjectName, "0");
}
