using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Roots_", menuName = "Skill/Roots")]
public class Roots : Skill
{
    public LayerMask EnemyLayer;
    public float rootDuration = 2f;
    public float enemyCountForRoot = 2;

    public GameObject rootEffect; 

    public override bool Activate(GameObject user)
    {
        if (user == null) return false;
        var mana = user.GetComponent<PlayerMana>();
        if (mana != null && mana.CurrentMana < ManaCost)
        {
            Debug.Log("No mana");
            return false;
        }

        mana?.UseMana(ManaCost);
        
        var hits = Physics2D.OverlapCircleAll(
            user.transform.position,
            Radius,
            EnemyLayer
        );

        for (var i = 0; i < Math.Min(enemyCountForRoot, hits.Length); i++)
        {
            var rootable = hits[i].GetComponent<IRootable>();
            if (rootable != null)
            {
                rootable.ApplyRoot(rootDuration, rootEffect);
            }
        }

        return true;
    }
}
