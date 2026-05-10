using System;
using UnityEngine;

public class Totem : MonoBehaviour
{
    public enum TotemType
    {
        Heal = 0,
        Ice = 1,
        Fire = 2
    }

    [Header("Totem Settings")]
    public TotemType type;
    public float radius = 2.5f;

    [Header("Layers")]
    public LayerMask allyLayer;
    public LayerMask enemyLayer;

    [Header("Values")]
    public int healAmount = 10;
    public int damageAmount = 15;

    [Header("FX")]
    public bool destroyOnEnd = true;

    private ItemStatistics _itemStatistics;

    private void Start()
    {
        _itemStatistics = GetComponent<ItemStatistics>();
    }

    public void OnTotemEffect()
    {
        switch (type)
        {
            case TotemType.Heal:
                DoHeal();
                break;

            case TotemType.Ice:
            case TotemType.Fire:
                DoDamage();
                break;
        }
    }

    public void OnTotemEnd()
    {
        if (destroyOnEnd)
            Destroy(gameObject);
    }

    private void DoHeal()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, allyLayer);

        foreach (var h in hits)
        {
            var hp = h.GetComponent<EnemyStatistics>();
            if (hp != null)
            {
                hp.RestoreHealth(healAmount);
            }
        }
    }

    private void DoDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (var h in hits)
        {
            var hp = h.GetComponent<IDamageable>();
            if (hp != null)
            {
                hp.TakeDamage(damageAmount);

                if (type == TotemType.Ice)
                {
                    // ApplyIceDebuff(h.gameObject);
                }

                if (type == TotemType.Fire)
                {
                    ApplyFireDebuff(h.gameObject);
                }
            }
        }
    }

    //TODO
    // private void ApplyIceDebuff(GameObject target)
    // {
    //     var slow = target.GetComponent<StatusController>();
    //     if (slow != null)
    //     {
    //         slow.ApplySlow(0.5f, 2f); // np. 50% na 2 sekundy
    //     }
    // }

    private void ApplyFireDebuff(GameObject target)
    {
        if (_itemStatistics != null && _itemStatistics.Effect != null)
        {
            _itemStatistics.Effect.Apply(target, null, _itemStatistics.ChanceForHit);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
