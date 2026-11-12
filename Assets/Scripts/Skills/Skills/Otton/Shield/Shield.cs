using UnityEngine;

[CreateAssetMenu(fileName = "Shield_", menuName = "Skill/Otton/Shield")]
public class Shield : Skill
{
    [SerializeField] public GameObject ShieldEffect;
    [SerializeField] public float Duration;
    [SerializeField] public float ReduceAmount;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;
        
        var playerHealth = user.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("There is no player health for Shield Spell");
            return false;
        }
        
        var reduceAmount = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.ShieldReduction, ReduceAmount);
        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, Duration);
        
        playerHealth.ReduceDamage(reduceAmount, duration, ShieldEffect);
        return true;
    }

    public override string GetDescription()
    {
        var reduceAmount = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.ShieldReduction, ReduceAmount);
        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, Duration);
        var reductionPercent = reduceAmount * 100f;
        
        return
            $"Otton’s iron will forms an unyielding ward around you.\n\nFor {duration:0.#} seconds, the ward reduces incoming damage by {reductionPercent:0.#}%. Hold your ground while the storm breaks against your shield.";
    }
}
