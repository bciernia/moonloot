using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AuraOfKnowledge_", menuName = "Skill/Aulla/AuraOfKnowledge")]
public class AuraOfKnowledge : Skill
{
    public LayerMask EnemyLayer;
    public float showCooldownDuration = 5f;
    public float enemyCountForAura = 2;

    public override bool Activate(GameObject user)
    {
        if (user == null) return false;

        var mana = user.GetComponent<PlayerMana>();
        if (mana != null && !mana.TryUseMana(ManaCost)) return false;

        var hits = Physics2D.OverlapCircleAll(
            user.transform.position,
            Radius,
            EnemyLayer
        );
        
        Array.Sort(hits, (a, b) =>
            Vector2.SqrMagnitude(user.transform.position - a.transform.position)
                .CompareTo(
                    Vector2.SqrMagnitude(user.transform.position - b.transform.position)
                )
        );

        var count = Mathf.Min(enemyCountForAura, hits.Length);

        for (var i = 0; i < count; i++)
        {
            var showEnemyInfo = hits[i].GetComponentInChildren<IShowEnemyInfo>();
            if (showEnemyInfo != null)
            {
                showEnemyInfo.ShowEnemyCooldown(showCooldownDuration);
            }
        }

        return true;
    }
}
