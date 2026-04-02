using System;
using UnityEngine;

public class DecisionAttackRange : FSMDecision
{
    [SerializeField] public LayerMask playerMask;

    private EnemyBrain _enemyBrain;
    private EnemyStatistics _enemyStatistics;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override bool Decide()
    {
        return PlayerInAttackRange();
    }

    public bool PlayerInAttackRange()
    {
        if (!_enemyBrain.Player) return false;

        var target = _enemyBrain.Player;

        var distance = Vector2.Distance(
            transform.position,
            target.position
        );

        return distance <= _enemyStatistics.AttackRange + 0.1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, _enemyStatistics.AttackRange);
    }
}
