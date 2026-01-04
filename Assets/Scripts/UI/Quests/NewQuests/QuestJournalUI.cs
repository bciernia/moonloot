using UnityEngine;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestElementUI _questElementUI;
    private QuestJournal _questJournal;

    private void Start()
    {
        _questJournal = GameObject.FindGameObjectWithTag("Player")
            .GetComponent<QuestJournal>();

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

        transform.DetachChildren();

        foreach (var questStatus in _questJournal.GetQuestStatuses())
        {
            var uiInstance = Instantiate(_questElementUI, transform);
            uiInstance.Setup(questStatus);
        }
    }
}