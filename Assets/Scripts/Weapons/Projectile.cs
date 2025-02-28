using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;

    public Vector3 Direction { get; set; }

    public float Damage { get; set; }

    private void Update()
    {
        transform.Translate(Direction * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<IDamageable>()?.TakeDamage(Damage);
        Destroy(gameObject);
    }
}

