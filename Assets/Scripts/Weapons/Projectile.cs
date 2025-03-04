using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    private Collider2D _collider;
    
    public Vector3 Direction { get; set; }
    public float Damage { get; set; }
    public GameObject Shooter { get; set; }
    public bool IsEnemy { get; set; }

    private void Start()
    {
        if (Shooter != null)
        {
            var shooterCollider = Shooter.GetComponent<Collider2D>();
            if (shooterCollider != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooterCollider, true);
            }
        }
    }
    
    private void Update()
    {
        transform.Translate(Direction * (speed * Time.deltaTime));
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == Shooter) return;
        
        other.GetComponent<IDamageable>()?.TakeDamage(Damage);
        other.GetComponent<KnockBack>()?.GetKnockedBack(transform, 5f);
        Destroy(gameObject);
    }
}

