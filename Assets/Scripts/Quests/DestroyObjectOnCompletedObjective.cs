using UnityEngine;

public class DestroyObjectOnCompletedObjective : MonoBehaviour
{
    [SerializeField] private QuestCompletion _questCompletion;
    [SerializeField] private string objectiveId;

    private void OnEnable()
    {
        _questCompletion.OnObjectiveCompleted += HandleObjectiveCompleted;
    }

    private void OnDisable()
    {
        _questCompletion.OnObjectiveCompleted -= HandleObjectiveCompleted;
    }

    private void HandleObjectiveCompleted(string completedObjectiveId)
    {
        if (completedObjectiveId == objectiveId)
        {
            Destroy(gameObject);
        }
    }
}
