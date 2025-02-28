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

    private bool PlayerInAttackRange()
    {
        if (!_enemyBrain.Player) return false;
        var playerCollider = Physics2D.OverlapCircle(_enemyBrain.transform.position, _enemyStatistics.AttackRange, playerMask);

        if (playerCollider)
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyStatistics.AttackRange);
    }
}
