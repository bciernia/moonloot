using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Roots_", menuName = "Skill/Sylvar/Roots")]
public class Roots : Skill
{
    public LayerMask EnemyLayer;
    public float enemyCountForRoot = 2;

    public GameObject rootEffect;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        var targetCount =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.TargetCount,
                enemyCountForRoot
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

        Array.Sort(hits, (a, b) =>
            Vector2.SqrMagnitude(user.transform.position - a.transform.position)
                .CompareTo(
                    Vector2.SqrMagnitude(user.transform.position - b.transform.position)
                )
        );

        for (var i = 0; i < Math.Min(targetCount, hits.Length); i++)
        {
            var rootable = hits[i].GetComponent<IRootable>();

            if (rootable != null)
            {
                rootable.ApplyRoot(duration, rootEffect);
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
                enemyCountForRoot
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
            $"Sylvar answers with a snare of ancient roots.\n\nThe {targetCount:0} nearest enemies within a radius of {radius:0.#} units are rooted in place for {duration:0.#} seconds, giving you time to strike or flee. The forest remembers every trespass.";
    }
}
