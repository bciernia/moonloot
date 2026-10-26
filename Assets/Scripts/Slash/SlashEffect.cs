using System.Collections.Generic;
using UnityEngine;

public class SlashEffect: MonoBehaviour
{
    private readonly int weaponType = Animator.StringToHash("WeaponType");

    [SerializeField] private GameObject bloodParticle;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _sizedSpriteRenderer;
    private Transform _sizedSpriteTransform;
    private BoxCollider2D _boxCollider;
    private bool _isTriggered;
    private float _rangeBonus;
    private Vector2 _hitboxLocalCenterOffset;
    
    public WeaponItemSO weapon;
    public float speed = 3f;
    private Transform _parent;
    private GameObject _shooter;

    private HashSet<GameObject> _hitTargets = new();
    
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

        if (!weapon.ProjectilePrefab)
        {
            SetBoxCollider(weapon.SlashSize, weapon.SlashOffset, _rangeBonus);
            CreateSizedVisual();
        }
    }

    public void SetRangeBonus(float rangeBonus)
    {
        _rangeBonus = Mathf.Max(0f, rangeBonus);
    }

    private void SetBoxCollider(
        Vector2 weaponColliderSize,
        Vector2 weaponColliderOffset,
        float rangeBonus)
    {
        var extendedSize = weaponColliderSize;
        var extendedOffset = weaponColliderOffset;

        if (rangeBonus > 0f)
        {
            var localRangeBonus = rangeBonus /
                                  Mathf.Max(Mathf.Abs(transform.lossyScale.x), 0.0001f);

            // Extend by the requested world-space range while keeping the near edge fixed.
            extendedSize.x += localRangeBonus;
            extendedOffset.x += localRangeBonus * 0.5f;
        }

        _boxCollider.size = extendedSize;
        _hitboxLocalCenterOffset = extendedOffset;
        _boxCollider.offset = Vector2.zero;
    }

    private void CreateSizedVisual()
    {
        if (_spriteRenderer == null || _boxCollider == null)
            return;

        var visual = new GameObject("SizedSlashVisual");
        _sizedSpriteTransform = visual.transform;
        _sizedSpriteTransform.SetParent(transform, false);

        _sizedSpriteRenderer = visual.AddComponent<SpriteRenderer>();
        _sizedSpriteRenderer.drawMode = SpriteDrawMode.Simple;
        _sizedSpriteRenderer.sharedMaterial = _spriteRenderer.sharedMaterial;
        _sizedSpriteRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        _sizedSpriteRenderer.sortingOrder = _spriteRenderer.sortingOrder;
        _sizedSpriteRenderer.color = _spriteRenderer.color;
        _sizedSpriteRenderer.maskInteraction = _spriteRenderer.maskInteraction;
        _sizedSpriteRenderer.spriteSortPoint = _spriteRenderer.spriteSortPoint;

        // The root Animator continues to control the source renderer's sprites.
        // A second renderer mirrors those frames and can be scaled independently.
        _spriteRenderer.enabled = false;
        UpdateSizedVisual();
    }

    private void LateUpdate()
    {
        UpdateSizedVisual();
    }

    private void UpdateSizedVisual()
    {
        if (_sizedSpriteRenderer == null || _spriteRenderer == null ||
            _sizedSpriteTransform == null || _boxCollider == null)
        {
            return;
        }

        var currentSprite = _spriteRenderer.sprite;
        _sizedSpriteRenderer.sprite = currentSprite;
        _sizedSpriteRenderer.flipX = _spriteRenderer.flipX;
        _sizedSpriteRenderer.flipY = _spriteRenderer.flipY;

        if (currentSprite == null)
        {
            _sizedSpriteRenderer.enabled = false;
            return;
        }

        _sizedSpriteRenderer.enabled = true;
        var visibleBounds = GetVisibleSpriteBounds(currentSprite);
        var spriteSize = visibleBounds.size;
        var scaleX = spriteSize.x > 0f
            ? _boxCollider.size.x / spriteSize.x
            : 1f;
        var scaleY = spriteSize.y > 0f
            ? _boxCollider.size.y / spriteSize.y
            : 1f;

        _sizedSpriteTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        var spriteCenter = visibleBounds.center;
        if (_sizedSpriteRenderer.flipX)
            spriteCenter.x *= -1f;
        if (_sizedSpriteRenderer.flipY)
            spriteCenter.y *= -1f;

        _sizedSpriteTransform.localPosition = new Vector3(
            -spriteCenter.x * scaleX,
            -spriteCenter.y * scaleY,
            0f);
    }

    private static Bounds GetVisibleSpriteBounds(Sprite sprite)
    {
        var vertices = sprite.vertices;
        if (vertices == null || vertices.Length == 0)
            return sprite.bounds;

        var bounds = new Bounds(vertices[0], Vector3.zero);
        for (var i = 1; i < vertices.Length; i++)
        {
            bounds.Encapsulate(vertices[i]);
        }

        return bounds;
    }

    public void EnableCollider()
    {
        _boxCollider.enabled = true;
    }
    
    private void OnEnable()
    {
        _hitTargets.Clear();
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
            transform.rotation = _parent.rotation;

            var scale = transform.lossyScale;
            var colliderCenterOffset = new Vector3(
                _hitboxLocalCenterOffset.x * scale.x,
                _hitboxLocalCenterOffset.y * scale.y,
                0f);

            transform.position = _parent.position +
                                transform.rotation * colliderCenterOffset;
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

        if (!_hitTargets.Add(other.gameObject)) return;

        var playerDmg = _shooter.GetComponent<Player>().PlayerAttack.GetPlayerDamage();
        
        SoundManager.Instance.PlaySound(weapon.HitSoundType);
        other.GetComponent<IDamageable>()?.TakeDamage(playerDmg, _shooter.transform);
        CombatStatsManager.Instance.DamageDealt += playerDmg;
        
        if(weapon.Effect) weapon.Effect.Apply(other.gameObject, null, weapon.EffectChance);
        
        if (bloodParticle != null && other.gameObject.CompareTag("Enemy"))
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
        if (!Application.isPlaying)
            return;

        var collider = _boxCollider != null
            ? _boxCollider
            : GetComponent<BoxCollider2D>();

        if (collider == null || !collider.enabled)
            return;

        var previousMatrix = Gizmos.matrix;
        var previousColor = Gizmos.color;
        var gizmoSize = new Vector3(
            collider.size.x,
            collider.size.y,
            0.01f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.15f, 0.1f, 0.2f);
        Gizmos.DrawCube(collider.offset, gizmoSize);
        Gizmos.color = new Color(1f, 0.15f, 0.1f, 1f);
        Gizmos.DrawWireCube(collider.offset, gizmoSize);

        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
} 
