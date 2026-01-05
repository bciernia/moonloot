using UnityEngine;

public class QuestAddTriggerAfterComplete : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStatistics[] enemies;

    [SerializeField] private QuestSO questForEnemies;
    [SerializeField] private string entryToAdd;
    
    [SerializeField] private QuestSO newQuestAfterKilling;
    [SerializeField] private string entryToAddForNewQuest;
    
    private bool triggered = false;

    private QuestJournal _questJournal;
    
    private void Awake()
    {
        enemies = GetComponentsInChildren<EnemyStatistics>();
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
    }

    private void Update()
    {
        if (triggered) return;

        var allDead = true;
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.CurrentHP > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            triggered = true;
            CompleteObjectiveAndTryAddQuest();
        }
    }

    private void CompleteObjectiveAndTryAddQuest()
    {
        if (_questJournal == null)
        {
            return;
        }
        
        _questJournal.AddEntryToJournal(questForEnemies, entryToAdd);
        _questJournal.AddQuest(newQuestAfterKilling, entryToAddForNewQuest);        

        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}