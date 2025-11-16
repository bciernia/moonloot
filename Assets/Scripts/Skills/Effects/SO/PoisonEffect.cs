using UnityEngine;

[CreateAssetMenu(fileName = "Poisoning", menuName = "Effects/Poisoning")]
public class PoisonEffect : Effect
{
    public float TotalDamage;

    protected override void OnTick(GameObject target)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(TotalDamage * TickInterval / Duration);
        }
    }
}