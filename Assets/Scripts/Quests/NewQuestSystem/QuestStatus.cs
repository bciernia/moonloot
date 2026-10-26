using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class QuestStatus
{
    [SerializeField] public string _questID;
    [SerializeField] private List<string> _questEntries = new List<string>();
    [SerializeField] private List<int> _objectiveProgress = new List<int>();
    [SerializeField] private string _giverID;
    [SerializeField] private string _giverName;
    [SerializeField] private string _giverProfession;
    [SerializeField] private string _giverPortraitName;
    [System.NonSerialized] private Sprite _giverPortrait;
    [SerializeField] public bool IsQuestCompleted { get; private set; } = false;
    
    private QuestSO _quest;
    
    public QuestStatus(
        QuestSO quest,
        string giverID = "",
        string giverName = "",
        Sprite giverPortrait = null,
        string giverProfession = "")
    {
        _questID = quest.Id;
        _quest = quest;
        _giverID = giverID ?? string.Empty;
        _giverName = giverName ?? string.Empty;
        _giverProfession = giverProfession ?? string.Empty;
        _giverPortrait = giverPortrait;
        _giverPortraitName = giverPortrait != null ? giverPortrait.name : string.Empty;
        EnsureProgressList();
    }

    public string GiverID => _giverID;
    public string GiverName => _giverName;
    public string GiverProfession => _giverProfession;
    public Sprite GiverPortrait
    {
        get
        {
            if (_giverPortrait == null && !string.IsNullOrWhiteSpace(_giverPortraitName))
            {
                foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                {
                    if (sprite != null && sprite.name == _giverPortraitName)
                    {
                        _giverPortrait = sprite;
                        break;
                    }
                }
            }

            return _giverPortrait;
        }
    }
    public string GiverPortraitName => _giverPortraitName;
    public bool AreObjectivesComplete => GetQuest() != null &&
                                         GetQuest().GetObjectives().Count > 0 &&
                                         AllObjectivesComplete();
    public bool IsReadyToTurnIn => !IsQuestCompleted && AreObjectivesComplete;

    public QuestSO GetQuest()
    {
        if (_quest == null)
            _quest = QuestDatabase.Get(_questID);

        return _quest;
    }
    public void AddQuestEntry(string journalEntry)
    {
        if (!string.IsNullOrWhiteSpace(journalEntry) && !_questEntries.Contains(journalEntry))
            _questEntries.Add(journalEntry);
    }

    public bool HasQuestEntry(string journalEntry) => _questEntries.Contains(journalEntry);

    public void FinishQuest() => IsQuestCompleted = true;

    public int GetObjectiveProgress(int index)
    {
        EnsureProgressList();
        return index >= 0 && index < _objectiveProgress.Count ? _objectiveProgress[index] : 0;
    }

    public bool AddObjectiveProgress(int index, int amount)
    {
        var quest = GetQuest();
        if (IsQuestCompleted || quest == null || amount <= 0 ||
            index < 0 || index >= quest.GetObjectives().Count)
            return false;

        EnsureProgressList();
        var objective = quest.GetObjectives()[index];
        var previous = _objectiveProgress[index];
        _objectiveProgress[index] = Mathf.Min(
            Mathf.Max(1, objective.requiredAmount),
            previous + amount);
        return _objectiveProgress[index] != previous;
    }

    public void RestoreProgress(List<int> progress)
    {
        EnsureProgressList();
        if (progress == null)
            return;

        for (var i = 0; i < _objectiveProgress.Count && i < progress.Count; i++)
        {
            var objective = GetQuest().GetObjectives()[i];
            _objectiveProgress[i] = Mathf.Clamp(progress[i], 0, Mathf.Max(1, objective.requiredAmount));
        }
    }

    public List<int> GetObjectiveProgressValues()
    {
        EnsureProgressList();
        return new List<int>(_objectiveProgress);
    }

    public string GetObjectiveProgressSummary()
    {
        var quest = GetQuest();
        if (quest == null)
            return string.Empty;

        var summary = new StringBuilder();
        var objectives = quest.GetObjectives();
        for (var i = 0; i < objectives.Count; i++)
        {
            if (summary.Length > 0)
                summary.AppendLine();

            var objective = objectives[i];
            if (objective.type == QuestObjectiveType.CompleteMap)
            {
                summary.Append(string.IsNullOrWhiteSpace(objective.sceneName)
                    ? "Complete the map"
                    : $"Complete {objective.sceneName.Trim()}");
                continue;
            }

            summary.Append(quest.GetObjectiveShortText(i));
            summary.Append(' ');
            summary.Append(GetObjectiveProgress(i));
            summary.Append('/');
            summary.Append(Mathf.Max(1, objective.requiredAmount));
        }

        if (IsQuestCompleted)
            summary.AppendLine("Completed");
        else if (AreObjectivesComplete && !string.IsNullOrWhiteSpace(_giverName))
            summary.AppendLine($"Return to {_giverName}");

        return summary.ToString().TrimEnd();
    }

    private bool AllObjectivesComplete()
    {
        var objectives = GetQuest().GetObjectives();
        for (var i = 0; i < objectives.Count; i++)
        {
            if (GetObjectiveProgress(i) < Mathf.Max(1, objectives[i].requiredAmount))
                return false;
        }

        return true;
    }

    private void EnsureProgressList()
    {
        if (_objectiveProgress == null)
            _objectiveProgress = new List<int>();

        var objectiveCount = _quest != null ? _quest.GetObjectives().Count : 0;
        while (_objectiveProgress.Count < objectiveCount)
            _objectiveProgress.Add(0);
        if (_objectiveProgress.Count > objectiveCount)
            _objectiveProgress.RemoveRange(objectiveCount, _objectiveProgress.Count - objectiveCount);
    }

    public List<string> GetQuestEntries => _questEntries;
}
