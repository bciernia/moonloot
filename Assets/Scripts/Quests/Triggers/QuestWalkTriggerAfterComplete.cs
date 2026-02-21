using UnityEngine;

public class QuestWalkTriggerAfterComplete : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStatistics[] enemies;
    [SerializeField] private WalkAfterAction objectTrigger;
    [SerializeField] private QuestSO quest;
    
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
            TriggerWalkOnNPC();
        }
    }

    private void TriggerWalkOnNPC()
    {
        if (objectTrigger == null)
        {
            return;
        }

        objectTrigger.TriggerWalk();
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}