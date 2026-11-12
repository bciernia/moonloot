using UnityEngine;

[CreateAssetMenu(fileName = "Heal_", menuName = "Skill/Heal")]
public class Heal : Skill
{
    [SerializeField] private float healAmount;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        base.Activate(user);

        return true;
    }

    public override string GetDescription()
    {
        var hpAmount = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.HealAmount, healAmount);
        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, ActiveTime);

        var tickInterval = Effect != null ? Effect.TickInterval : 1f;
        var tickCount = tickInterval > 0f ? Mathf.CeilToInt(duration / tickInterval) : 0;
        var totalHpAmount = hpAmount * tickCount;

        return
            $"Sylvar calls the forest’s life back into your wounds.\n\nRestore {hpAmount:0.#} Health each second for {duration:0.#} seconds, up to {totalHpAmount:0.#} Health in total. The healing arrives over time, so keep clear of danger while it takes hold.";
    }
}
