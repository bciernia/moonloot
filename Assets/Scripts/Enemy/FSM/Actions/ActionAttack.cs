using System;
using UnityEngine;

public class ActionAttack : FSMAction
{
    private EnemyStatistics _enemyStatistics;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    public float _timer;
    
    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override void Act()
    {
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (!_enemyBrain.Player) return;

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _enemyAnimator.SetAttackAnimation();
            _timer = _enemyStatistics.TimeBetweenAttacks;
        } 
    }

    public void DealDmgToPlayer()
    {
        var player = _enemyBrain.Player.GetComponent<IDamageable>();
        player.TakeDamage(_enemyStatistics.Damage);
    }
}