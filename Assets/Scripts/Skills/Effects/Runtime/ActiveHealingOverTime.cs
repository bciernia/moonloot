using UnityEngine;
using System.Collections;

public class ActiveHealingOverTime : ActiveEffect
{
    private float healPerTick;
    private float tickInterval;
    private IHealable target;

    public void InitializeEffect(HealingEffect effect, GameObject obj)
    {
        target = obj.GetComponent<IHealable>();

        if (target == null)
        {
            Destroy(this);
            return;
        }

        duration = effect.Duration;
        tickInterval = effect.TickInterval;
        healPerTick = effect.TotalHeal / (duration / tickInterval);

        StartCoroutine(Healing());
    }

    private IEnumerator Healing()
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            target.RestoreHealth(healPerTick);
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        Destroy(this);
    }
}