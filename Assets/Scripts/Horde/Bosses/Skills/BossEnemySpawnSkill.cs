using UnityEngine;

public class BossEnemySpawnSkill : MonoBehaviour, IBossSkill
{
    [Header("Skill")]
    [SerializeField] private float _healthThreshold = 0.75f;
    [SerializeField] private float _cooldown = 8f;
    [SerializeField] private int _enemiesToSpawn = 3;

    [Header("Spawn")]
    [SerializeField] private float _spawnRadius = 5f;

    private float _cooldownTimer;

    public float HealthThreshold => _healthThreshold;
    public bool ExecuteOnce => false;

    private void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    public bool CanExecute()
    {
        return _cooldownTimer <= 0f;
    }

    public void Execute()
    {
        _cooldownTimer = _cooldown;

        for (var i = 0; i < _enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        var spawnPosition = GetSpawnPosition();

        HordeManager.Instance.SpawnEnemyFromBoss(spawnPosition);
    }

    private Vector3 GetSpawnPosition()
    {
        var offset = Random.insideUnitCircle * _spawnRadius;

        return transform.position +
               new Vector3(offset.x, offset.y, 0f);
    }
}