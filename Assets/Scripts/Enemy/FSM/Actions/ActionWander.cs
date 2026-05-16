using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ActionWander : FSMAction
{
    [SerializeField] private float speed;
    [SerializeField] private float wanderTime;
    [SerializeField] private Vector2 moveRange;

    private EnemyStatistics _enemyStatistics;
    private EnemyAnimator _enemyAnimator;
    private NavMeshAgent _navMeshAgent;

    private Vector3 movePosition;
    private float timer;

    private void Awake()
    {
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // WAŻNE dla 2D NavMesh
        if (_navMeshAgent != null)
        {
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.updateUpAxis = false;
        }

        GetNewDestination();
    }

    public override void Act()
    {
        timer -= Time.deltaTime;

        Vector3 moveDirection =
            (movePosition - transform.position).normalized;

        Vector3 movement =
            moveDirection * (_enemyStatistics.Speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePosition) >= 1f)
        {
            _enemyAnimator.SetIsMoving(true);

            _enemyAnimator.SetMoveAnimation(
                new Vector2(moveDirection.x, moveDirection.y));

            transform.Translate(movement);

            // SYNCHRONIZACJA Z NAVMESH AGENT
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.Warp(transform.position);
            }
        }
        else
        {
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

        for (int i = 0; i < attempts; i++)
        {
            float randomX =
                Random.Range(-moveRange.x, moveRange.x);

            float randomY =
                Random.Range(-moveRange.y, moveRange.y);

            Vector3 potentialPosition =
                transform.position +
                new Vector3(randomX, randomY, 0f);

            Collider2D hit =
                Physics2D.OverlapCircle(potentialPosition, 0.5f);

            if (hit == null ||
                hit.CompareTag(TagTypes.Environment) == false)
            {
                movePosition = potentialPosition;
                return;
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if (moveRange != Vector2.zero)
    //     {
    //         Gizmos.color = Color.cyan;
    //         Gizmos.DrawWireCube(transform.position, moveRange * 2f);
    //         Gizmos.DrawLine(transform.position, movePosition);
    //     }
    // }
}