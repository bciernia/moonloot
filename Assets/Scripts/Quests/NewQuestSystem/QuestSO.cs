using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Quest configuration")]
    [SerializeField] private string _name;
    [SerializeField][TextArea] private string _description;
    [SerializeField] private List<Quest.Reward> _rewards = new List<Quest.Reward>();
    [SerializeField] private List<ItemSO> _questItems = new List<ItemSO>();

    [Header("Quest json")] [SerializeField]
    private TextAsset _questJson; 
    
    private Dictionary<string, Dictionary<string, string>> localizationData;

    public string GetQuestTitle() => _name ?? name;

    public string GetQuestDescription() => _description ?? string.Empty;

    public IEnumerable<Quest.Reward> GetRewards() => _rewards;
    public IEnumerable<ItemSO> GetQuestItems() => _questItems;

    private void LoadData()
    {
        if (_questJson == null)
        {
            Debug.LogError($"Missing JSON for quest {_name}");
            return;
        }
        
        localizationData  = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(_questJson.text);
    }
    
    public string GetEntry(string key, string languageCode = "en")
    {
        if (localizationData == null)
            LoadData();

        if (localizationData != null &&
            localizationData.TryGetValue(languageCode, out var entries) &&
            entries.TryGetValue(key, out var text))
        {
            return text;
        }

        return $"[Missing: {key} ({languageCode})]";
    }
}
