using UnityEngine;

public class ActionRetreat : FSMAction
{
    private EnemyAnimator _enemyAnimator;
    private EnemyStatistics _enemyStatistics;
    public Vector2 destinationRange;
    private Vector3 movePosition;

    private float timer;
    private readonly float changeLocationTime = .5f;
    private void Awake()
    {
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override void Act()
    {
        timer -= Time.deltaTime;
        var moveDirection = (movePosition - transform.position).normalized;
        var movement = moveDirection * (_enemyStatistics.ChaseSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, movePosition) >= .25f)
        {
            _enemyAnimator.FlipSpriteXOff();
            _enemyAnimator.SetIsMoving(true);
            _enemyAnimator.SetMoveAnimation(new Vector2(moveDirection.x, moveDirection.y));
            transform.Translate(movement);
        }
        else
        {
            _enemyAnimator.TryFlipSpriteX();
            _enemyAnimator.SetIsMoving(false);
        }
        
        if (timer <= 0f)
        {
            GetNewDestination();
            timer = changeLocationTime;
        }
    }
    
    private void GetNewDestination()
    {
        var randomX = Random.Range(-destinationRange.x, destinationRange.x);
        var randomY = Random.Range(-destinationRange.y, destinationRange.y);
        movePosition = transform.position + new Vector3(randomX, randomY);
    }
}
