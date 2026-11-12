using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Confusion_", menuName = "Skill/Nythera/Confusion")]
public class Confusion : Skill
{
    public LayerMask EnemyLayer;
    public float enemyCountForConfusion = 3;

    public GameObject rootEffect;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        var targetCount =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.TargetCount,
                enemyCountForConfusion
            );

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                ActiveTime
            );

        var radius =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Radius,
                Radius
            );

        var hits = Physics2D.OverlapCircleAll(
            user.transform.position,
            radius,
            EnemyLayer
        );

        for (var i = 0; i < Math.Min(targetCount, hits.Length); i++)
        {
            var confusion = hits[i].GetComponent<IConfusionable>();

            if (confusion != null)
            {
                confusion.ApplyConfusion(duration, rootEffect);
            }
        }

        return true;
    }

    public override string GetDescription()
    {
        var targetCount =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.TargetCount,
                enemyCountForConfusion
            );

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                ActiveTime
            );

        var radius =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Radius,
                Radius
            );
        
        return
            $"Nythera buries your presence beneath a veil of whispers.\n\nUp to {targetCount:0} enemies within a radius of {radius:0.#} units lose track of you and abandon their pursuit for {duration:0.#} seconds. The spell deals no damage; use the opening to escape or reposition.";
    }
}
