using System;

public static class EnemyEvents
{
    public static event Action<EnemyStatistics> EnemyKilled;

    public static void RaiseEnemyKilled(
        EnemyStatistics enemy)
    {
        EnemyKilled?.Invoke(enemy);
    }
}