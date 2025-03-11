using System;
using UnityEngine;

public class DecisionPerformedAttack : FSMDecision
{
    private EnemyBrain _enemyBrain;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    public override bool Decide()
    {
        return CanGoToNewLocation();
    }

    private bool CanGoToNewLocation()
    {
        return _enemyBrain.AttackCooldown > 0;
    }
}
