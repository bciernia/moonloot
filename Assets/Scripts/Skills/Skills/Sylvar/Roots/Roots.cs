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

        for (var i = 0; i < Math.Min(enemyCountForRoot, hits.Length); i++)
        {
            var rootable = hits[i].GetComponent<IRootable>();
            if (rootable != null)
            {
                rootable.ApplyRoot(ActiveTime, rootEffect);
            }
        }

        return true;
    }
}
