using UnityEngine;
using UnityEngine.Serialization;

public class SlashEffect: MonoBehaviour
{
    private readonly int weaponType = Animator.StringToHash("WeaponType");

    [SerializeField] private GameObject bloodParticle;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private bool _isTriggered;
    
    public WeaponItemSO weapon;
    public float speed = 3f;
    private Transform _parent;
    private GameObject _shooter;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isTriggered = AttackManager.Instance.IsTriggered;
        _boxCollider = GetComponent<BoxCollider2D>();

        _spriteRenderer.flipY = _isTriggered;
        _rb.linearVelocity = transform.right;
        _animator.speed = speed;
        _animator.SetFloat(weaponType, (int)weapon.WeaponType);

        if(!weapon.ProjectilePrefab) SetBoxCollider(weapon.SlashSize, weapon.SlashOffset);
    }

    private void SetBoxCollider(Vector2 weaponColliderSize, Vector2 weaponColliderOffset)
    {
        _boxCollider.size = weaponColliderSize;
        _boxCollider.offset = weaponColliderOffset;
    }

    public void EnableCollider()
    {
        _boxCollider.enabled = true;
    }
    
    public void DisableCollider()
    {
        _boxCollider.enabled = false;
    }
    
    public void SetParent(Transform newParent)
    {
        _parent = newParent;
    }

    private void Update()
    {
        if (_parent != null)
        {
            transform.position = _parent.position;
            transform.rotation = _parent.rotation;
        }
    }

    private void SetIsTriggered()
    {
        _isTriggered = !_isTriggered;
        AttackManager.Instance.IsTriggered = _isTriggered;
    }
    
    public void SetShooter(GameObject shooter)
    {
        _shooter = shooter;
    }

    private void FireProjectile()
    {
        var projectile = Instantiate(weapon.ProjectilePrefab, transform.position, transform.rotation);
        projectile.Shooter = _shooter;
        projectile.IsEnemy = false;
        projectile.Direction = Vector3.right;
        projectile.Damage = weapon.Damage;
    }

    public void DestroySelf()
    {
        SetIsTriggered();
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanAttack(other)) return;
        
        other.GetComponent<IDamageable>()?.TakeDamage(weapon.Damage);
        other.GetComponent<KnockBack>()?.GetKnockedBack(transform, 5f);
        
        if(weapon.Effect) weapon.Effect.Apply(other.gameObject, weapon.EffectChance);
        
        if (bloodParticle != null)
        {
            var blood = Instantiate(bloodParticle, other.transform.position, Quaternion.identity);
            blood.GetComponent<BloodParticle>()?.SpawnBlood(other.transform.position, transform.position);
        }
    }

    private bool CanAttack(Collider2D target)
    {
        if (target.gameObject == _shooter) return false;
        if (weapon.ProjectilePrefab) return false;

        var enemyStats = target.GetComponent<EnemyStatistics>();
        var itemStats = target.GetComponent<ItemStatistics>();

        if (enemyStats != null)
            return enemyStats.CurrentHP > 0;

        if (itemStats != null)
            return itemStats.CurrentHP > 0;

        return false;
    }
    
    private void OnDrawGizmos()
    {
        // Jeżeli collider nie istnieje w tym momencie — spróbuj go pobrać
        if (_boxCollider == null)
            _boxCollider = GetComponent<BoxCollider2D>();

        if (_boxCollider == null) return;

        Gizmos.color = Color.red;

        // Pozycja kolizji w world space
        Vector3 worldCenter = transform.TransformPoint(_boxCollider.offset);

        // Rozmiar w world space (2D box → rysujemy jako 3D cube)
        Vector3 worldSize = new Vector3(
            _boxCollider.size.x * Mathf.Abs(transform.lossyScale.x),
            _boxCollider.size.y * Mathf.Abs(transform.lossyScale.y),
            0.01f // cienkie Z, żeby było widać
        );

        Gizmos.DrawWireCube(worldCenter, worldSize);
    }
} 