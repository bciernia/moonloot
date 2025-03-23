using System;
using UnityEngine;
using UnityEngine.AI;

public class ActionChase : FSMAction
{
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private EnemyStatistics _enemyStatistics;

    private NavMeshAgent _navMeshAgent;
    
    private bool CanMove { get; set; }

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis= false;
        CanMove = true;
    }

    public override void Act()
    {
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (!_enemyBrain.Player) return;

        _navMeshAgent.SetDestination(_enemyBrain.Player.position);
        
        _enemyAnimator.FlipSpriteXOff();
        //
        // var dirToPlayer = _enemyBrain.Player.position - transform.position;
        //
        // //odległość w jakiej się zatrzymują przed graczem
        // if (dirToPlayer.magnitude >= _enemyStatistics.StopRange && CanMove)
        // {
        //     transform.Translate(dirToPlayer.normalized * (_enemyStatistics.ChaseSpeed * Time.deltaTime));
        // }
    }

    public void DisableMove()
    {
        CanMove = false;
    }
    
    public void EnableMove()
    {
        CanMove = true;
    }
}
