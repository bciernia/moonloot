using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestElementUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _objectiveProgress;
    [SerializeField] private TextMeshProUGUI _giver;
    [SerializeField] private TextMeshProUGUI _giverProfession;
    [SerializeField] private TextMeshProUGUI _story;
    [SerializeField] private Image _giverPortrait;
    [SerializeField] private Image _background;
    [SerializeField] private Color _notStartedColor = new Color(0.75f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color _inProgressColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private Color _completedColor = new Color(0.2f, 0.65f, 0.25f, 1f);
    [SerializeField] private bool _hidePortraitWhenMissing = true;
    [SerializeField] private bool _showRequiredItems = true;

    private QuestDescription _questDescription;

    private void Awake()
    {
        _questDescription = GetComponent<QuestDescription>();
    }
    
    private QuestStatus QuestStatus { get; set; }

    public void Setup(QuestStatus questStatus)
    {
        QuestStatus = questStatus;

        if (questStatus == null)
        {
            if (_objectiveProgress != null) _objectiveProgress.text = string.Empty;
            if (_giver != null) _giver.text = string.Empty;
            if (_giverProfession != null) _giverProfession.text = string.Empty;
            if (_story != null) _story.text = string.Empty;
            if (_giverPortrait != null) _giverPortrait.enabled = false;
            return;
        }

        if (_objectiveProgress != null)
        {
            var quest = questStatus.GetQuest();
            var hasObjectives = quest != null && quest.GetObjectives().Count > 0;

            if (questStatus.IsReadyToTurnIn)
            {
                _objectiveProgress.text = string.IsNullOrWhiteSpace(questStatus.GiverName)
                    ? "Return to the quest giver"
                    : $"Return to {questStatus.GiverName}";
            }
            else
            {
                // Objective progress is the live value. Required-item data is only
                // a fallback for older quests that have no tracked objectives.
                _objectiveProgress.text = hasObjectives
                    ? questStatus.GetObjectiveProgressSummary()
                    : _showRequiredItems ? GetRequiredItemsText(questStatus) : string.Empty;
            }
        }

        if (_giver != null)
            _giver.text = questStatus.GiverName;

        if (_giverProfession != null)
            _giverProfession.text = questStatus.GiverProfession;

        if (_story != null)
            _story.text = GetQuestStory();

        if (_giverPortrait != null)
        {
            var portrait = questStatus.GiverPortrait;
            _giverPortrait.sprite = portrait;
            _giverPortrait.enabled = portrait != null || !_hidePortraitWhenMissing;
        }

        if (questStatus.IsQuestCompleted)
        {
            SetBackgroundColor(_completedColor);
        }
        else if (HasObjectiveProgress(questStatus))
        {
            SetBackgroundColor(_inProgressColor);
        }
        else
        {
            SetBackgroundColor(_notStartedColor);
        }
    }

    public string GetQuestStory()
    {
        return QuestStatus != null
            ? QuestStatus.GetQuest()?.GetQuestStory() ?? string.Empty
            : string.Empty;
    }

    private void SetBackgroundColor(Color color)
    {
        if (_background != null)
            _background.color = color;
    }

    private static bool HasObjectiveProgress(QuestStatus status)
    {
        var quest = status.GetQuest();
        if (quest == null)
            return false;

        for (var i = 0; i < quest.GetObjectives().Count; i++)
        {
            if (status.GetObjectiveProgress(i) > 0)
                return true;
        }

        return false;
    }

    private static string GetRequiredItemsText(QuestStatus status)
    {
        var quest = status.GetQuest();
        if (quest == null)
            return string.Empty;

        var lines = new System.Text.StringBuilder();
        foreach (var requiredItem in quest.GetQuestItems())
        {
            var item = requiredItem.item.item;
            if (item == null)
                continue;

            if (lines.Length > 0)
                lines.AppendLine();

            var amount = requiredItem.number > 0
                ? requiredItem.number
                : Mathf.Max(1, requiredItem.item.quantity);
            lines.Append(item.Name);
            lines.Append(" x");
            lines.Append(amount);
        }

        return lines.Length > 0 ? lines.ToString() : status.GetObjectiveProgressSummary();
    }

    public void SetQuestDescription()
    {
        _questDescription.SetQuestDescription(QuestStatus);
    }
}
