using System;
using UnityEngine;

public class ActionAttack : FSMAction
{
    private EnemyStatistics _enemyStatistics;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private DecisionAttackRange _decisionAttackRange;

    private bool HasAttacked { get; set; }
    
    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _decisionAttackRange = GetComponent<DecisionAttackRange>();

        HasAttacked = false;
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
            HasAttacked = false;
        } 
    }

    public void DealDmgToPlayer()
    {
        if (!_decisionAttackRange.PlayerInAttackRange() || HasAttacked) return; 
        
        var playerDamage = _enemyBrain.Player.GetComponent<IDamageable>();
        
        playerDamage.TakeDamage(_enemyStatistics.Damage);

        HasAttacked = true;

        AddStateForPlayer();
        
        KnockBackPlayer();
    }

    private void KnockBackPlayer()
    {
        var playerKnockBack = _enemyBrain.Player.GetComponent<KnockBack>();

        if (playerKnockBack == null) return;
        
        playerKnockBack.GetKnockedBack(transform, 5f);
    }

    private void AddStateForPlayer()
    {
        if (_enemyStatistics != null && _enemyStatistics.Effect != null)
        {
            _enemyStatistics.Effect.Apply(_enemyBrain.Player.gameObject,null, _enemyStatistics.EffectChance);
        }
    }
}