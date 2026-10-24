using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Quest")]
public class QuestSO : IdentifiableSO
{
    [Header("Quest configuration")]
    [SerializeField] private string _name;
    [Tooltip("What the player needs to do. Existing quests can keep using this as their description.")]
    [SerializeField][TextArea] private string _description;
    [SerializeField][TextArea] private string _storyDescription;
    [SerializeField] private List<QuestObjective> _objectives = new List<QuestObjective>();
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

    [Serializable]
    public class QuestObjective
    {
        public QuestObjectiveType type;
        [Tooltip("Optional custom short text, e.g. 'Find moonstone'. If empty, text is generated from the target.")]
        public string shortText;
        [Min(1)] public int requiredAmount = 1;
        public EnemyStatsSO enemyType;
        public ItemSO item;
        [Tooltip("Exact scene name, e.g. ForestMap. Leaving that scene counts as completing it.")]
        public string sceneName;
    }
    
    public string GetQuestTitle() => _name ?? name;

    public string GetQuestDescription() => _description ?? string.Empty;

    public string GetStoryDescription() => _storyDescription ?? string.Empty;

    public IEnumerable<Reward> GetRewards() => _rewards;
    public IEnumerable<QuestItem> GetQuestItems() => _questItems;
    public IReadOnlyList<QuestObjective> GetObjectives()
    {
        if (_objectives == null)
            return Array.Empty<QuestObjective>();

        return _objectives;
    }

    public string GetObjectiveShortText(int index)
    {
        if (_objectives == null || index < 0 || index >= _objectives.Count)
            return string.Empty;

        var objective = _objectives[index];
        if (!string.IsNullOrWhiteSpace(objective.shortText))
            return objective.shortText.Trim();

        switch (objective.type)
        {
            case QuestObjectiveType.KillEnemy:
                return $"Kill {GetEnemyName(objective.enemyType)}";
            case QuestObjectiveType.CollectItem:
                return $"Find {GetItemName(objective.item)}";
            case QuestObjectiveType.CompleteMap:
                return $"Complete {objective.sceneName}";
            default:
                return "Objective";
        }
    }

    private static string GetEnemyName(EnemyStatsSO enemy) =>
        enemy != null && !string.IsNullOrWhiteSpace(enemy.Name) ? enemy.Name : "enemies";

    private static string GetItemName(ItemSO item) =>
        item != null && !string.IsNullOrWhiteSpace(item.Name) ? item.Name : "items";

    private void LoadData()
    {
        if (_questJson == null)
        {
            return;
        }
        
        localizationData  = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(_questJson.text);
    }
    
    public string GetEntry(string key, string languageCode = "en")
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (localizationData == null)
            LoadData();

        if (localizationData != null &&
            localizationData.TryGetValue(languageCode, out var entries) &&
            entries.TryGetValue(key, out var text))
        {
            return text;
        }

        if (_questJson == null)
            return string.Empty;

        return $"[Missing: {key} ({languageCode})]";
    }

    public Guid guid { get; }

    public string GetQuestStory() => _storyDescription;
}
