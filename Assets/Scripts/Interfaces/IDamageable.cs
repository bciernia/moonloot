using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Transform damageSourceTransform = null, DamageType type = DamageType.Physical);
}
