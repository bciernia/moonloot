using System;
using UnityEngine;

public class ActionAttack : FSMAction
{
    private EnemyStatistics _enemyStatistics;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private DecisionAttackRange _decisionAttackRange;
    
    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _decisionAttackRange = GetComponent<DecisionAttackRange>();
    }

    public override void Act()
    {
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (!_enemyBrain.Player) return;

        if (_enemyBrain.CanAttack())
        {
            _enemyAnimator.SetAttackAnimation();
            _enemyBrain.ResetAttackCooldown();
        } 
    }

    public void DealDmgToPlayer()
    {
        if (!_decisionAttackRange.PlayerInAttackRange()) return; 
        
        var player = _enemyBrain.Player.GetComponent<IDamageable>();
        player.TakeDamage(_enemyStatistics.Damage);
    }
}