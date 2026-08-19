using UnityEngine;

public class BossSpawnEliteSkill : MonoBehaviour, IBossSkill
{
    [Header("Skill")]
    [SerializeField] private float _healthThreshold = 0.5f;

    [Header("Elite")]
    [SerializeField] private GameObject _elitePrefab;
    [SerializeField] private float _spawnDistance = 3f;

    public float HealthThreshold => _healthThreshold;
    public bool ExecuteOnce => true;

    public bool CanExecute()
    {
        return _elitePrefab != null;
    }

    public void Execute()
    {
        var offset = Random.insideUnitCircle.normalized * _spawnDistance;

        var spawnPosition = transform.position + new Vector3(
            offset.x,
            offset.y,
            0f
        );

        HordeManager.Instance.SpawnBossElite(spawnPosition);
    }
}