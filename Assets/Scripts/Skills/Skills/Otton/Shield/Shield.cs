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
        
        return
            $"Otton’s iron will manifests around you.\nFor {duration} seconds, incoming damage is reduced by {reduceAmount}%, as if your flesh were tempered steel.\n\nStand firm. Let the storm break upon you.";
    }
}