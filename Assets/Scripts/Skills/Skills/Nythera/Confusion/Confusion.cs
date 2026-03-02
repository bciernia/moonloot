using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Confusion_", menuName = "Skill/Nythera/Confusion")]
public class Confusion : Skill
{
    public LayerMask EnemyLayer;
    public float rootDuration = 2f;
    public float enemyCountForConfusion = 3;
    
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

        for (var i = 0; i < Math.Min(enemyCountForConfusion, hits.Length); i++)
        {
            var confusion = hits[i].GetComponent<IConfusionable>();
            if (confusion != null)
            {
                confusion.ApplyConfusion(rootDuration, rootEffect);
            }
        }

        return true;
    }
}