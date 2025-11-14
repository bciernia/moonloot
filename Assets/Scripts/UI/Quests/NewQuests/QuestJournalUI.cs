using UnityEngine;
using UnityEngine.UI;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestElementUI _questElementUI;
    private QuestJournal _questJournal;

    private void Start()
    {
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
        _questJournal.onUpdate += Redraw;
        Redraw();
    }

    private void Redraw()
    {
        transform.DetachChildren();
        foreach (var questStatus in _questJournal.GetQuestStatuses())
        {
            var uiInstance = Instantiate(_questElementUI, transform);
            //TODO zmiana koloru jeśli sie udało/nie udało
            var button = uiInstance.GetComponent<Button>();
            uiInstance.Setup(questStatus);
        }
    }
}