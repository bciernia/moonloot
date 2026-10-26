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
    [Header("NPC dialogue")]
    [Tooltip("What the quest giver says while this quest is active and still needs work.")]
    [SerializeField][TextArea] private string _activeQuestDialogue;
    [Tooltip("NPC line shown immediately after this quest is successfully turned in.")]
    [SerializeField][TextArea] private string _turnInDialogue;
    [Tooltip("Random lines the quest giver can say after the player completes this quest.")]
    [SerializeField][TextArea(2, 5)] private List<string> _completedQuestDialogueLines = new List<string>();
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

    public string GetActiveQuestDialogue()
    {
        if (!string.IsNullOrWhiteSpace(_activeQuestDialogue))
            return _activeQuestDialogue.Trim();

        if (!string.IsNullOrWhiteSpace(_storyDescription))
            return _storyDescription.Trim();

        return GetQuestDescription();
    }

    public string GetTurnInDialogue()
    {
        if (!string.IsNullOrWhiteSpace(_turnInDialogue))
            return _turnInDialogue.Trim();

        return $"Thank you for completing {GetQuestTitle()}. Your help means a great deal to me.";
    }

    public string GetRandomCompletedDialogueLine()
    {
        if (_completedQuestDialogueLines != null)
        {
            var usableLines = _completedQuestDialogueLines
                .FindAll(line => !string.IsNullOrWhiteSpace(line));
            if (usableLines.Count > 0)
                return usableLines[UnityEngine.Random.Range(0, usableLines.Count)].Trim();
        }

        var questTitle = GetQuestTitle();
        var fallbackLines = new[]
        {
            $"I still remember how you helped me with {questTitle}. Thank you.",
            $"People are still talking about what you did for {questTitle}.",
            $"I couldn't have finished {questTitle} without you.",
            $"Things are much better now that {questTitle} is behind us. Thanks again."
        };

        return fallbackLines[UnityEngine.Random.Range(0, fallbackLines.Length)];
    }

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
