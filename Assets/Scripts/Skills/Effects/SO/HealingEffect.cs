using UnityEngine;

[CreateAssetMenu(fileName = "Healing", menuName = "Effects/Healing")]
public class HealingEffect : Effect
{
    public float TotalHeal;

    protected override void OnTick(GameObject target)
    {
        var healable = target.GetComponent<IHealable>();
        if (healable != null)
        {
            var healAmount = TotalHeal * TickInterval / Duration;
            healable.RestoreHealth(healAmount);
        }
    }
}

