using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Compact in-game quest tracker, e.g. "Find rocks 6/10".
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _trackerText;
    [SerializeField, Min(1)] private int _maxVisibleObjectives = 3;

    private QuestJournal _questJournal;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        _questJournal = player != null ? player.GetComponent<QuestJournal>() : null;

        if (_questJournal == null)
        {
            Debug.LogWarning("QuestTrackerUI could not find QuestJournal on the Player.", this);
            return;
        }

        _questJournal.onUpdate += Redraw;
        Redraw();
    }

    private void OnDestroy()
    {
        if (_questJournal != null)
            _questJournal.onUpdate -= Redraw;
    }

    private void Redraw()
    {
        if (_trackerText == null || _questJournal == null)
            return;

        var text = new StringBuilder();
        var visibleCount = 0;

        foreach (var status in _questJournal.GetQuestStatuses())
        {
            if (status.IsQuestCompleted)
                continue;

            var quest = status.GetQuest();
            if (quest == null)
                continue;

            for (var i = 0; i < quest.GetObjectives().Count; i++)
            {
                if (visibleCount >= _maxVisibleObjectives)
                    break;

                if (text.Length > 0)
                    text.AppendLine();

                text.Append(quest.GetObjectiveShortText(i));
                text.Append(' ');
                text.Append(status.GetObjectiveProgress(i));
                text.Append('/');
                text.Append(Mathf.Max(1, quest.GetObjectives()[i].requiredAmount));
                visibleCount++;
            }

            if (visibleCount >= _maxVisibleObjectives)
                break;
        }

        _trackerText.text = text.ToString();
        gameObject.SetActive(visibleCount > 0);
    }
}
