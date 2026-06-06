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
        
        return
            $"A hush falls upon the minds of your enemies.\nUp to {targetCount} foes lose all interest in you, abandoning their aggression and returning to their patrol as if you never existed for {duration} seconds.\n\nNythera does not command fear — she erases intent.";
    }
}