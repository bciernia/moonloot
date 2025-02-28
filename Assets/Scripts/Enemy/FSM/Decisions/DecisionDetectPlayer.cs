using System;
using UnityEngine;

public class DecisionDetectPlayer : FSMDecision
{
    [SerializeField] public float range;
    [SerializeField] public LayerMask playerMask;

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
            _enemyAnimator.SetIsMoving(true);
            var directionToEnemy = (_enemyBrain.Player.position - _enemyBrain.transform.position).normalized;
            _enemyAnimator.SetMoveAnimation(new Vector2(directionToEnemy.x, directionToEnemy.y));
            return true;
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
