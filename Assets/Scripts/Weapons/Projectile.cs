using System;
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
    
    private void Start()
    {
        _startPosition = transform.position;

        if (ProjectileSo != null)
        {
            _maxDistanceSqr = ProjectileSo.MaxDistance * ProjectileSo.MaxDistance;
        }
        
        if (Shooter != null)
        {
            var shooterCollider = Shooter.GetComponent<Collider2D>();
            if (shooterCollider != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooterCollider, true);
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
        if (other.gameObject == Shooter || other.gameObject.CompareTag("CameraBound") || other.gameObject.CompareTag("CameraBoundQuest") || other.gameObject.CompareTag("NPC")) return;

        SoundManager.Instance.PlaySound(ProjectileSo.HitSound);
        other.GetComponent<IDamageable>()?.TakeDamage(Damage);
        other.GetComponent<KnockBack>()?.GetKnockedBack(transform, ProjectileSo.KnockBackThrust);
        
        if(ProjectileSo.Effect) ProjectileSo.Effect.Apply(other.gameObject, ProjectileSo.EffectChance);

        //Zrobić particle dla innych materiałów (drewno/kamień)
        if (bloodParticle != null && other.gameObject.CompareTag("Enemy"))
        {
            var blood = Instantiate(bloodParticle, other.transform.position, Quaternion.identity);
            blood.GetComponent<BloodParticle>()?.SpawnBlood(other.transform.position, transform.position);
        }
        
        Destroy(gameObject);
    }
}

