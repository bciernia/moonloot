using UnityEngine;

public class CombatStatsManager : Singleton<CombatStatsManager>
{
    public float DamageDealt;
    public int EnemiesKilled;
    public float DistanceTraveled;

    private Vector3 _lastPlayerPosition;
    
    public void ResetStats(Transform player)
    {
        DamageDealt = 0;
        EnemiesKilled = 0;
        DistanceTraveled = 0f;

        _lastPlayerPosition = player.position;
    }

    public void UpdateDistance(Transform player)
    {
        DistanceTraveled += Vector3.Distance(player.position, _lastPlayerPosition);
        _lastPlayerPosition = player.position;
    }
}
