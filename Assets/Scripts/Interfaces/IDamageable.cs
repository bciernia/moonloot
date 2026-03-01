using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, DamageType type = DamageType.Physical);
}
