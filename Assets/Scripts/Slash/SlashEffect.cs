using UnityEngine;

public class SlashEffect: MonoBehaviour
{
    private readonly int weaponType = Animator.StringToHash("WeaponType");
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isTriggered;
    
    public WeaponSO weapon;
    public float speed = 3f;
    private Transform parent;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isTriggered = AttackManager.Instance.IsTriggered;

        spriteRenderer.flipY = isTriggered;
        rb.linearVelocity = transform.right;
        animator.speed = speed;
        animator.SetFloat(weaponType, (int)weapon.WeaponType);
    }
    
    public void SetParent(Transform newParent)
    {
        parent = newParent;
    }

    private void Update()
    {
        if (parent != null)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
    }

    private void SetIsTriggered()
    {
        isTriggered = !isTriggered;
        AttackManager.Instance.IsTriggered = isTriggered;
    }

    private void FireProjectile()
    {
        var projectile = Instantiate(weapon.ProjectilePrefab, transform.position, transform.rotation);
        projectile.Direction = Vector3.right;
        projectile.Damage = weapon.Damage;
    }

    public void DestroySelf()
    {
        SetIsTriggered();
        Destroy(gameObject);
    }
} 