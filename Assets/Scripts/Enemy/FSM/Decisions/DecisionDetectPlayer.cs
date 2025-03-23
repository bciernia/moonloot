using System;
using UnityEngine;

public class DecisionDetectPlayer : FSMDecision
{
    [SerializeField] public float range;
    [SerializeField] public LayerMask playerMask;
    [SerializeField] public LayerMask obstacleMask;

    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
    }

    public override bool Decide()
    {
        return DetectPlayer();
    }

    private bool DetectPlayer()
    {
        var playerCollider = Physics2D.OverlapCircle(_enemyBrain.transform.position, range, playerMask);

        if (playerCollider)
        {
            _enemyBrain.Player = playerCollider.transform;
            var directionToEnemy = (_enemyBrain.Player.position - _enemyBrain.transform.position).normalized;
            var distanceToPlayer = Vector2.Distance(_enemyBrain.transform.position, playerCollider.transform.position);

            var hit = Physics2D.Raycast(_enemyBrain.transform.position, directionToEnemy, distanceToPlayer, obstacleMask);

            // if (!hit.collider)
            // {
                _enemyAnimator.SetIsMoving(true);
                _enemyAnimator.SetMoveAnimation(new Vector2(directionToEnemy.x, directionToEnemy.y));
                return true;
            // }
        }
        
        _enemyBrain.Player = null;
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
