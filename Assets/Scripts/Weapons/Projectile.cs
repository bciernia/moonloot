using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private GameObject bloodParticle;
    
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
        if (other.gameObject == Shooter || other.gameObject.CompareTag("CameraBound")) return;

        other.GetComponent<IDamageable>()?.TakeDamage(Damage);
        //Knockbackthrust przenieść do właściwości pocisku
        other.GetComponent<KnockBack>()?.GetKnockedBack(transform, 5f);
        
        //Zrobić particle dla innych materiałów (drewno/kamień)
        if (bloodParticle != null && !other.gameObject.CompareTag(TagTypes.Environment))
        {
            var blood = Instantiate(bloodParticle, other.transform.position, Quaternion.identity);
            blood.GetComponent<BloodParticle>()?.SpawnBlood(other.transform.position, transform.position);
        }
        
        Destroy(gameObject);
    }
}

