using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject bloodParticle;
    
    public Vector3 Direction { get; set; }
    public float Damage { get; set; }
    public GameObject Shooter { get; set; }
    public bool IsEnemy { get; set; }

    public ProjectileSO ProjectileSo;

    private Vector3 _startPosition;
    private float _maxDistanceSqr;
    
    [SerializeField] private float bounceRange = 20f;

    private int _remainingBounces;
    private GameObject _lastHitTarget;
    
    private void Start()
    {
        _startPosition = transform.position;

        if (ProjectileSo != null)
        {
            _maxDistanceSqr = ProjectileSo.MaxDistance * ProjectileSo.MaxDistance;

            if (ProjectileSo.CanBounce)
                _remainingBounces = ProjectileSo.BounceNumber;
        }

        if (Shooter != null)
        {
            var shooterCollider = Shooter.GetComponent<Collider2D>();
            if (shooterCollider != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooterCollider, true);
            }

            if (Shooter.CompareTag("Player"))
            {
                Damage = Shooter.GetComponent<Player>().PlayerAttack.GetPlayerDamage();
            }
            else
            {
                Damage = Shooter.GetComponent<EnemyStatistics>()?.Damage ?? ProjectileSo.Damage;
            }

            if (Damage == 0)
            {
                Damage = ProjectileSo.Damage;
            }
        }
    }
    
    private void Update()
    {
        transform.Translate(Direction * (ProjectileSo.Speed * Time.deltaTime));

        if (ProjectileSo == null || !(ProjectileSo.MaxDistance > 0f)) return;
        
        var distanceSqr = (transform.position - _startPosition).sqrMagnitude;

        if (distanceSqr >= _maxDistanceSqr)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == Shooter ||
            other.gameObject.CompareTag("CameraBound") ||
            other.gameObject.CompareTag("CameraBoundQuest") ||
            other.gameObject.CompareTag("NPC") ||
            other.gameObject.CompareTag("DroppedItem"))
            return;

        if (other.gameObject == _lastHitTarget)
            return;

        SoundManager.Instance.PlaySound(ProjectileSo.HitSound);

        other.GetComponent<IDamageable>()?.TakeDamage(Damage, Shooter.transform);
        other.GetComponent<KnockBack>()?.GetKnockedBack(transform, ProjectileSo.KnockBackThrust);

        if (Shooter.CompareTag("Player"))
            CombatStatsManager.Instance.DamageDealt += Damage;

        if (ProjectileSo.Effect)
            ProjectileSo.Effect.Apply(other.gameObject, null, ProjectileSo.EffectChance);

        if (bloodParticle != null && other.gameObject.CompareTag("Enemy"))
        {
            var blood = Instantiate(bloodParticle, other.transform.position, Quaternion.identity);
            blood.GetComponent<BloodParticle>()?.SpawnBlood(other.transform.position, transform.position);
        }

        _lastHitTarget = other.gameObject;

        if (ProjectileSo.CanBounce && _remainingBounces > 0)
        {
            var nextTarget = FindNextTarget(_lastHitTarget);

            if (nextTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            _remainingBounces--;

            Direction = (nextTarget.transform.position - transform.position).normalized;

            transform.position += Direction * 0.2f;

            return;
        }

        Destroy(gameObject);
    }
    
    private GameObject FindNextTarget(GameObject currentTarget)
    {
        var allEnemies = FindObjectsOfType<EnemyStatistics>();

        var closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (var enemy in allEnemies)
        {
            if (!enemy.IsAlive)
                continue;

            var go = enemy.gameObject;

            if (go == currentTarget)
                continue;

            var dist = (go.transform.position - transform.position).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = go;
            }
        }

        return closest;
    }
}

