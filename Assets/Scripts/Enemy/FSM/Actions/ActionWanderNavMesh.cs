using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ActionWanderNavMesh : FSMAction
{
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float minDistanceToPoint = 1f;
    [SerializeField] private float waitTime = 2f;

    private NavMeshAgent _navMeshAgent;
    private EnemyAnimator _enemyAnimator;
    private EnemyStatistics _enemyStatistics;

    private float _waitTimer;
    private Vector3 _currentDestination;
    private bool _hasDestination;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    private void Start()
    {
        EnemyNavMeshAgent.EnableNavMeshAgent(_navMeshAgent);

        _navMeshAgent.speed = _enemyStatistics.Speed;

        GetNewDestination();
    }

    public override void Act()
    {
        if (!_navMeshAgent.enabled)
            return;

        _navMeshAgent.speed = _enemyStatistics.Speed;

        if (!_hasDestination)
        {
            _waitTimer -= Time.deltaTime;

            _enemyAnimator.SetIsMoving(false);

            if (_waitTimer <= 0f)
            {
                GetNewDestination();
            }

            return;
        }

        var velocity = _navMeshAgent.velocity.normalized;

        if (_navMeshAgent.velocity.magnitude > 0.1f)
        {
            _enemyAnimator.SetIsMoving(true);
            _enemyAnimator.SetMoveAnimation(
                new Vector2(velocity.x, velocity.y));
        }
        else
        {
            _enemyAnimator.SetIsMoving(false);
        }

        var distance = Vector3.Distance(
            transform.position,
            _currentDestination);

        if (distance <= minDistanceToPoint)
        {
            _hasDestination = false;
            _waitTimer = waitTime;
        }
    }

    private void GetNewDestination()
    {
        const int attempts = 15;

        for (var i = 0; i < attempts; i++)
        {
            var randomDirection = Random.insideUnitCircle * wanderRadius;

            var targetPosition = transform.position +
                                 new Vector3(randomDirection.x,
                                     randomDirection.y,
                                     0f);

            if (NavMesh.SamplePosition(
                    targetPosition,
                    out var hit,
                    2f,
                    NavMesh.AllAreas))
            {
                _currentDestination = hit.position;

                _navMeshAgent.SetDestination(_currentDestination);

                _hasDestination = true;

                return;
            }
        }

        _hasDestination = false;
    }
}