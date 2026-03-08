using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Quest")]
public class QuestSO : IdentifiableSO
{
    [Header("Quest configuration")]
    [SerializeField] private string _name;
    [SerializeField][TextArea] private string _description;
    [SerializeField] private List<Reward> _rewards = new List<Reward>();
    [SerializeField] private List<QuestItem> _questItems = new List<QuestItem>();

    [Header("Quest json")] [SerializeField]
    private TextAsset _questJson; 
    
    private Dictionary<string, Dictionary<string, string>> localizationData;

    [Serializable]
    public class Reward
    {
        [Min(1)]
        public int number;
        public InventoryItem item;
    }

    [Serializable]
    public class QuestItem
    {
        [Min(1)]
        public int number;
        public InventoryItem item;
    }
    
    public string GetQuestTitle() => _name ?? name;

    public string GetQuestDescription() => _description ?? string.Empty;

    public IEnumerable<Reward> GetRewards() => _rewards;
    public IEnumerable<QuestItem> GetQuestItems() => _questItems;

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

    public Guid guid { get; }
}
