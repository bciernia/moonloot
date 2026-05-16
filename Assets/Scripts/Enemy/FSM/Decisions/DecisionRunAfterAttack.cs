using System;
using UnityEngine;

public class DecisionRunAfterAttack : FSMDecision
{
    private EnemyStatistics _enemyStatistics;
    
    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override bool Decide()
    {
        return _enemyStatistics.ShouldRunAway;
    }
}
