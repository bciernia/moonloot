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

    // private void ChasePlayer()
    // {
    //     if (!_enemyBrain.Player) return;
    //
    //     if (_enemyStatistics._isRooted)
    //     {
    //         _navMeshAgent.isStopped = true;
    //         _enemyAnimator.SetIsMoving(false);
    //         return;
    //     }
    //     
    //     var distanceToPlayer = Vector3.Distance(transform.position, _enemyBrain.Player.position);
    //
    //     
    //     
    //     if (distanceToPlayer > _enemyStatistics.StopRange)
    //     {
    //         _navMeshAgent.isStopped = false;
    //         _navMeshAgent.SetDestination(_enemyBrain.Player.position);
    //         _enemyAnimator.SetIsMoving(true);
    //     }
    //     else
    //     {
    //         _navMeshAgent.isStopped = true;
    //         _enemyAnimator.SetIsMoving(false);
    //     }
    //
    //     _enemyAnimator.FlipSpriteXOff();
    //     
    //     var dirToPlayer = _enemyBrain.Player.position - transform.position;
    //     
    //     //odległość w jakiej się zatrzymują przed graczem
    //     if (dirToPlayer.magnitude >= _enemyStatistics.StopRange && CanMove)
    //     {
    //         transform.Translate(dirToPlayer.normalized * (_enemyStatistics.ChaseSpeed * Time.deltaTime));
    //     }
    // }

    private void ChasePlayer() 
    {
        if (!_enemyBrain.Player) return;
        
        if (!_navMeshAgent.isOnNavMesh)
            return;
        
        if (_enemyStatistics._isRooted || DialogueManager.Instance.IsInDialogue())
        {
            _navMeshAgent.isStopped = true;
            _enemyAnimator.SetIsMoving(false);
            return;
        }
        
        var distanceToPlayer = Vector3.Distance(
            transform.position,
            _enemyBrain.Player.position
        );

        _navMeshAgent.speed = _enemyStatistics.ChaseSpeed;

        if (distanceToPlayer > _enemyStatistics.StopRange)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_enemyBrain.Player.position);
            _enemyAnimator.SetIsMoving(true);
        }
        else
        {
            _navMeshAgent.isStopped = true;
            _enemyAnimator.SetIsMoving(false);
        }

        _enemyAnimator.FlipSpriteXOff();
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
