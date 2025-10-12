using System;
using UnityEngine;

public class DestroyObjectOnCompletedObjective : MonoBehaviour
{
    [SerializeField] private Quest _quest;
    [SerializeField] private string objectiveId;

    private QuestList _questList;
    
    private void Awake()
    {
        _questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
    }

    private void OnEnable()
    {
        _questList.OnObjectiveCompleted += HandleObjectiveCompleted;
    }

    private void OnDisable()
    {
        _questList.OnObjectiveCompleted -= HandleObjectiveCompleted;
    }

    private void HandleObjectiveCompleted(Quest quest, string completedObjectiveId)
    {
        if (_quest.GetTitle() == quest.GetTitle() && completedObjectiveId == objectiveId)
        {
            Destroy(gameObject);
        }
    }
}
