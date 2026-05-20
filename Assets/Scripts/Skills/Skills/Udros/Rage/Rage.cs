using UnityEngine;

[CreateAssetMenu(fileName = "Rage_", menuName = "Skill/Rage")]
public class Rage : Skill
{
    public float DmgMultiplier = 2f;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var hp = user.GetComponent<PlayerHealth>();
        if (hp != null && hp.CurrentHealth <= HealthCost)
        {
            Debug.Log("Hp is too low");
            FloatingTextManager.Instance.ShowWarningText("HP is too low", user.transform);
            return false;
        }

        hp?.TakeDamage(HealthCost, null,  DamageType.True);

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                ActiveTime
            );
        
        var damageMultiplier =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.DamageMultiplier,
                DmgMultiplier
            );
        
        var playerAttack = user.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.ApplyDmgMultiplier(damageMultiplier, duration);
        }

        return true;
    }
}
