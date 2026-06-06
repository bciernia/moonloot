using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AuraOfKnowledge_", menuName = "Skill/Aulla/AuraOfKnowledge")]
public class AuraOfKnowledge : Skill
{
    public LayerMask EnemyLayer;
    public float showCooldownDuration = 5f;
    public float enemyCountForAura = 2;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        var targetCount =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.TargetCount,
                enemyCountForAura
            );

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                showCooldownDuration
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

        var count = Mathf.Min((int)targetCount, hits.Length);

        for (var i = 0; i < count; i++)
        {
            var showEnemyInfo =
                hits[i].GetComponentInChildren<IShowEnemyInfo>();

            if (showEnemyInfo != null)
            {
                showEnemyInfo.ShowEnemyCooldown(duration);
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
                enemyCountForAura
            );

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                showCooldownDuration
            );
        
        return
            $"You pierce the illusion of time itself.\nUp to {targetCount} enemies within range are marked with prophetic sigils, revealing the exact moment of their next strike for {duration} seconds.\n\nKnowledge is not power — it is survival.";
    }
}