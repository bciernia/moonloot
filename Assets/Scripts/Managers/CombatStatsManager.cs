using UnityEngine;

public class CombatStatsManager : Singleton<CombatStatsManager>
{
    public float DamageDealt;
    public int NormalEnemiesKilled;
    public int EliteEnemiesKilled;
    public int BossEnemiesKilled;
    public float DistanceTraveled;
    public int GoldEarned;

    private Vector3 _lastPlayerPosition;
    
    public void ResetStats(Transform player)
    {
        DamageDealt = 0;
        NormalEnemiesKilled = 0;
        EliteEnemiesKilled = 0;
        BossEnemiesKilled = 0;
        DistanceTraveled = 0f;
        GoldEarned = 0;

        _lastPlayerPosition = player.position;
    }

    public void UpdateDistance(Transform player)
    {
        DistanceTraveled += Vector3.Distance(player.position, _lastPlayerPosition);
        _lastPlayerPosition = player.position;
    }
}
