using UnityEngine;

public class ItemStatistics : MonoBehaviour, IDamageable
{
    [SerializeField] private ItemStatsSO _itemStatistics;
    [SerializeField] private ParticleSystem _destroyParticles;
    
    public string Name { get; private set; }
    [TextArea] public string Description { get; private set; }
    public float CurrentHP { get; private set; }
    public float MaxHP { get; private set; }
    public Effect Effect { get; private set; }

    public int ChanceForHit;
    
    private void Start()
    {
        Name = _itemStatistics.Name;
        Description = _itemStatistics.Description;
        CurrentHP = _itemStatistics.MaxHP;
        MaxHP = _itemStatistics.MaxHP;
        Effect = _itemStatistics.Effect;
        Effect.BasicChanceForHit = ChanceForHit;
    }
    
    public void TakeDamage(float amount)
    {
        CurrentHP = Mathf.Max(CurrentHP - amount, 0);
        DamageManager.Instance.ShowDamageText(amount, transform);

        if (CurrentHP <= 0f)
        {
            if (_destroyParticles == null) return;
            var particles = Instantiate(_destroyParticles, transform.position, Quaternion.identity);
            var main = particles.main;
            particles.Play();
            Destroy(particles.gameObject, main.duration + main.startLifetime.constantMax);     
            Destroy(gameObject);
        }
    }
}
