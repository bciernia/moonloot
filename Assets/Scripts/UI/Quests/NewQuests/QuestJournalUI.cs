using UnityEngine;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestElementUI _questElementUI;
    private QuestJournal _questJournal;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        _questJournal = player != null ? player.GetComponent<QuestJournal>() : null;

        if (_questJournal == null)
        {
            Debug.LogWarning("QuestJournalUI could not find QuestJournal on the Player.", this);
            return;
        }

        _questJournal.onUpdate += Redraw;
        Redraw();
    }

    private void OnDestroy()
    {
        if (_questJournal != null)
        {
            _questJournal.onUpdate -= Redraw;
        }
    }

    private void Redraw()
    {
        if (this == null) return;

        for (var i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        foreach (var questStatus in _questJournal.GetQuestStatuses())
        {
            var uiInstance = Instantiate(_questElementUI, transform);
            uiInstance.Setup(questStatus);
        }
    }
}
