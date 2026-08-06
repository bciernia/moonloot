using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Freezing", menuName = "Effects/Freezing")]
public class FreezingEffect : Effect
{
    public float TotalDamage;

    protected override void OnApply(GameObject target)
    {
        if (target.TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            playerMovement.SetEffectSpeedMultiplier(0f);
            return;
        }
        
        if (target.TryGetComponent<EnemyStatistics>(out var enemy))
        {
            enemy.ApplyFreeze(Duration, VisualPrefab);
        }
    }

    protected override void OnExpire(GameObject target)
    {
        var movement = target.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.SetEffectSpeedMultiplier(1f);
        }
    }

    protected override void OnTick(GameObject target)
    {
        var damageable = target.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(
                (float)Math.Round(
                    TotalDamage * TickInterval / Duration,
                    1));
        }
    }
}