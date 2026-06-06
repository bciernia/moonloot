using UnityEngine;

[CreateAssetMenu(fileName = "WolfSpeed_", menuName = "Skill/WolfSpeed")]
public class WolfSpeed : Skill
{
    public float SpeedMultiplier = 2f;
    
    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, ActiveTime);
        var speed = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.SpeedMultiplier, SpeedMultiplier);
        
        var movement = user.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ApplySpeedMultiplier(speed, duration);
        }

        return true;
    }

    public override string GetDescription()
    {
        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, ActiveTime);
        var speed = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.SpeedMultiplier, SpeedMultiplier);

        return
            $"The spirit of the hunt flows through you.\nYour movement speed is {speed} times increased for {duration} seconds, as swift and relentless as a prowling wolf.";
    }
}
