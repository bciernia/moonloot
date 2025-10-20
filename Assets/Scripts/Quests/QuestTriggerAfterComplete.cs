using UnityEngine;
using System.Collections.Generic;

public class QuestTriggerAfterComplete : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStatistics[] enemies;
    [SerializeField] private WalkAfterAction objectTrigger;

    private bool triggered = false;

    private void Awake()
    {
        enemies = GetComponentsInChildren<EnemyStatistics>();
    }

    private void Update()
    {
        if (triggered) return;

        bool allDead = true;
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