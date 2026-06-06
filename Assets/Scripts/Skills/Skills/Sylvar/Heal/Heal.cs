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

        var totalHpAmount = hpAmount * 5;

        return
            $"Life answers your call.\n\nEvery second for {duration} seconds, the forest restores {hpAmount} Health, healing a total of {totalHpAmount} Health.";
    }
}