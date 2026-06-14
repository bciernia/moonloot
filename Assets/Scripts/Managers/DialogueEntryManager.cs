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
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("NpcDialogueDictionary", NpcDialogueEntryDictionary, settings);
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
