using UnityEngine;

public class BossProjectileSkill : MonoBehaviour, IBossSkill
{
    [Header("Skill")]
    [SerializeField] private float _healthThreshold = 1f;

    [Header("Normal Attack")]
    [SerializeField] private float _normalAttackInterval = 2f;

    [Header("Burst Attack")]
    [SerializeField] private float _burstInterval = 8f;
    [SerializeField] private int _burstProjectileCount = 10;
    [SerializeField] private float _burstProjectileInterval = 0.08f;

    [Header("Projectile")]
    [SerializeField] private Projectile _projectilePrefab;

    private float _normalAttackTimer;
    private float _burstTimer;

    private int _burstProjectilesFired;
    private float _burstProjectileTimer;
    private bool _isBursting;

    public float HealthThreshold => _healthThreshold;
    public bool ExecuteOnce => false;

    private void Awake()
    {
        _burstTimer = _burstInterval;
    }
    
    private void Update()
    {
        UpdateNormalAttack();
        UpdateBurstAttack();
    }

    private void UpdateNormalAttack()
    {
        if (_isBursting)
            return;

        _normalAttackTimer -= Time.deltaTime;

        if (_normalAttackTimer > 0f)
            return;

        Shoot();

        _normalAttackTimer = _normalAttackInterval;
    }

    private void UpdateBurstAttack()
    {
        _burstTimer -= Time.deltaTime;

        if (!_isBursting)
        {
            if (_burstTimer > 0f)
                return;

            StartBurst();
            return;
        }

        _burstProjectileTimer -= Time.deltaTime;

        if (_burstProjectileTimer > 0f)
            return;

        Shoot();

        _burstProjectilesFired++;

        if (_burstProjectilesFired >= _burstProjectileCount)
        {
            _isBursting = false;
            _burstTimer = _burstInterval;
            return;
        }

        _burstProjectileTimer = _burstProjectileInterval;
    }

    public bool CanExecute()
    {
        return !_isBursting;
    }

    public void Execute()
    {
    }

    private void StartBurst()
    {
        _isBursting = true;
        _burstProjectilesFired = 0;
        _burstProjectileTimer = 0f;
    }

    private void Shoot()
    {
        if (_projectilePrefab == null)
            return;

        var player = Player.Instance;

        if (player == null)
            return;

        var direction =
            player.transform.position - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        var angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        var rotation =
            Quaternion.Euler(0f, 0f, angle);

        var projectile = Instantiate(
            _projectilePrefab,
            transform.position,
            rotation);

        projectile.Shooter = gameObject;
        projectile.IsEnemy = true;
        projectile.Direction = Vector3.right;
    }
}