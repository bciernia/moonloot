using System.Collections.Generic;

public class DialogueEntryManager : Singleton<DialogueEntryManager>, ISaveable
{
    private Dictionary<string, string> NpcDialogueEntryDictionary = new();

    public void UpdateNpcDialogueEntry(string dialogueEntryId, string gameObjectName)
    {
        NpcDialogueEntryDictionary[gameObjectName] = dialogueEntryId;
    }

    public string GetDialogueEntryIdByNpcName(string gameObjectName) => NpcDialogueEntryDictionary.GetValueOrDefault(gameObjectName, "0");
    
    public void Save()
    {
        ES3.Save("NpcDialogueDictionary", NpcDialogueEntryDictionary);
    }

    public void Load()
    {
        if (ES3.KeyExists("NpcDialogueDictionary"))
        {
            NpcDialogueEntryDictionary =
                ES3.Load<Dictionary<string, string>>("NpcDialogueDictionary");
        }
        else
        {
            NpcDialogueEntryDictionary = new Dictionary<string, string>();
        }    
    }
}
