using System;
using UnityEngine;

public class Spell : MonoBehaviour
{
    private Animator _animator;
    private Collider2D _collider;
    private readonly int play = Animator.StringToHash("Play");

    public GameObject Shooter { get; set; }
    public float Damage { get; set; }
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }
    
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
        
        _animator.SetTrigger(play);
    }

    private void Update()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f 
            && !_animator.IsInTransition(0))
        {
            Destroy(gameObject);
        }
    }

    public void ActivateCollider()
    {
        _collider.enabled = true;
    }
    
    
    public void DeactivateCollider()
    {
       _collider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == Shooter || other.gameObject.CompareTag("CameraBound") || other.gameObject.CompareTag("CameraBoundQuest")) return;

        other.GetComponent<IDamageable>()?.TakeDamage(Damage);    
    }

    public void DestroySpell()
    {
        Destroy(gameObject);
    }
}
