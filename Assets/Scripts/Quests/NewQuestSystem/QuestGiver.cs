using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private List<QuestSO> _quests;

    private QuestJournal _questJournal;

    private void Awake()
    {
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
    }

    public QuestSO GetQuest(int questIndex = 0)
    {
        return _quests[questIndex];
    }

    public List<QuestSO> GetQuests() => _quests;

    //Used in dialogues in Trigger Script nodes
    public void GiveQuest(int id, string entryKey)
    {
        _questJournal.AddQuest(_quests[id], entryKey);
    }
    
    public bool HasPlayerQuestSOInQuestList(int id = 0) => _questJournal.HasPlayerQuest(_quests[id]);
}