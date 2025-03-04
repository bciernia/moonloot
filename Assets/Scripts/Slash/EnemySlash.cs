
using UnityEngine;

public class EnemySlash : MonoBehaviour
{
    private readonly int weaponType = Animator.StringToHash("WeaponType");
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isTriggered;
    
    public WeaponSO weapon;
    public float speed = 3f;
    private Transform _parent;
    private GameObject _shooter;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isTriggered = AttackManager.Instance.IsTriggered;

        _spriteRenderer.flipY = _isTriggered;
        _rb.linearVelocity = transform.right;
        _animator.speed = speed;
        _animator.SetFloat(weaponType, (int)weapon.WeaponType);
    }

    public void SetParent(Transform newParent)
    {
        _parent = newParent;
    }

    public void SetShooter(GameObject shooter)
    {
        _shooter = shooter;
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

    private void FireProjectile()
    {
        var projectile = Instantiate(weapon.ProjectilePrefab, transform.position, transform.rotation);
        projectile.Shooter = _shooter;
        projectile.IsEnemy = true;
        projectile.Direction = Vector3.right;
        projectile.Damage = weapon.Damage;
    }

    public void DestroySelf()
    {
        SetIsTriggered();
        Destroy(gameObject);
    }
}
