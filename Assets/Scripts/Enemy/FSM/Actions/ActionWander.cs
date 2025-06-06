using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActionWander : FSMAction
{
    [SerializeField] private float speed;
    [SerializeField] private float wanderTime;
    [SerializeField] private Vector2 moveRange;

    private EnemyStatistics _enemyStatistics;
    
    private Vector3 movePosition;
    private float timer;

    private EnemyAnimator _enemyAnimator;

    private void Awake()
    {
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }
    
    private void Start()
    {
        GetNewDestination();
    }

    public override void Act()
    {
        timer -= Time.deltaTime;
        var moveDirection = (movePosition - transform.position).normalized;
        var movement = moveDirection * (_enemyStatistics.Speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, movePosition) >= 1f)
        {
            // _enemyAnimator.FlipSpriteXOff();
            _enemyAnimator.SetIsMoving(true);
            _enemyAnimator.SetMoveAnimation(new Vector2(moveDirection.x, moveDirection.y));
            transform.Translate(movement);
        }
        else
        {
            // _enemyAnimator.TryFlipSpriteX();
            _enemyAnimator.SetIsMoving(false);
        }

        if (timer <= 0f)
        {
            GetNewDestination();
            timer = wanderTime;
        }
    }

    private void GetNewDestination()
    {
        const int attempts = 10;

        for (var  i = 0; i < attempts; i++)
        {
            var randomX = Random.Range(-moveRange.x, moveRange.x);
            var randomY = Random.Range(-moveRange.y, moveRange.y);
            var potentialPosition = transform.position + new Vector3(randomX, randomY);

            var hit = Physics2D.OverlapCircle(potentialPosition, 0.5f); 

            if (hit == null || hit.CompareTag(TagTypes.Environment) == false)
            {
                movePosition = potentialPosition;
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (moveRange != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, moveRange * 2f);
            Gizmos.DrawLine(transform.position, movePosition);
        }
    }
}
