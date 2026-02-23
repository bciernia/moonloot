using UnityEngine;

public class EnableExitAfterFight : MonoBehaviour
{
    [SerializeField] private GameObject exitSign;

    private int aliveEnemies;

    private void Start()
    {
        exitSign.SetActive(false);

        var enemies = GetComponentsInChildren<EnemyStatistics>();
        aliveEnemies = enemies.Length;

        foreach (var enemy in enemies)
        {
            enemy.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath(EnemyStatistics enemy)
    {
        aliveEnemies--;

        enemy.OnDeath -= HandleEnemyDeath;

        if (aliveEnemies <= 0)
        {
            exitSign.SetActive(true);
        }
    }
}