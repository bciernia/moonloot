using System;
using UnityEngine;

public class DecisionAllyFighting : FSMDecision
{
    public float detectionRadius = 10f;
    public LayerMask allyLayerMask;
    private EnemyBrain _enemyBrain;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    public override bool Decide()
    {
        return IsAnyAllyFighting();
    }

    private bool IsAnyAllyFighting()
    {
        var alliesInRange = Physics2D.OverlapCircleAll(transform.position, detectionRadius, allyLayerMask);

        foreach (var ally in alliesInRange)
        {
            var allyBrain = ally.GetComponent<EnemyBrain>();

            if (allyBrain != null)
            {
                var currentState = allyBrain.CurrentState.ID;
                
                if (currentState == EnemyStates.Attack || currentState == EnemyStates.Chase || currentState == EnemyStates.ChangeLocation)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}