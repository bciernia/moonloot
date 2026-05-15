using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ActionRun : FSMAction
{
    [Header("Run Settings")]
    [SerializeField] private float runDistance = 10f;
    [SerializeField] private float stopDistance = 1.5f;

    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private EnemyStatistics _enemyStatistics;

    private NavMeshAgent _navMeshAgent;

    private Vector3 _runPosition;
    private bool _destinationSet;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyStatistics = GetComponent<EnemyStatistics>();

        _navMeshAgent = GetComponent<NavMeshAgent>();

        // 2D
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;

        // WAŻNE
        _navMeshAgent.updatePosition = true;
    }

    public override void Act()
    {
        RunFromPlayer();
    }

    private void RunFromPlayer()
    {
        if (!_enemyBrain.Player)
            return;

        if (!_navMeshAgent.isOnNavMesh)
            return;

        if (_enemyStatistics._isRooted)
        {
            _navMeshAgent.isStopped = true;
            _enemyAnimator.SetIsMoving(false);
            return;
        }

        if (!_destinationSet)
        {
            FindRunPosition();

            _navMeshAgent.SetDestination(_runPosition);

            _destinationSet = true;
        }

        _navMeshAgent.speed = _enemyStatistics.ChaseSpeed * 1.5f;

        float distance =
            Vector3.Distance(transform.position, _runPosition);

        Debug.Log("Distance To Run Position: " + distance);

        if (distance > stopDistance)
        {
            _navMeshAgent.isStopped = false;

            _enemyAnimator.SetIsMoving(true);

            Vector3 velocity =
                _navMeshAgent.velocity.normalized;

            _enemyAnimator.SetMoveAnimation(
                new Vector2(velocity.x, velocity.y));
        }
        else
        {
            Debug.Log("DOTARŁ DO POZYCJI UCIECZKI");

            _navMeshAgent.isStopped = true;

            _enemyAnimator.SetIsMoving(false);

            _enemyStatistics.StopRunningAway();

            _destinationSet = false;
        }

        _enemyAnimator.FlipSpriteXOff();
    }

    private void FindRunPosition()
    {
        const int attempts = 20;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 directionAwayFromPlayer =
                (transform.position - _enemyBrain.Player.position).normalized;

            Vector2 randomOffset =
                Random.insideUnitCircle * 3f;

            Vector3 targetPosition =
                transform.position +
                directionAwayFromPlayer * runDistance +
                new Vector3(randomOffset.x, randomOffset.y, 0f);

            if (NavMesh.SamplePosition(
                    targetPosition,
                    out NavMeshHit hit,
                    5f,
                    NavMesh.AllAreas))
            {
                _runPosition = hit.position;
                _runPosition.z = transform.position.z;

                Debug.Log("Run Position: " + _runPosition);

                return;
            }
        }

        _runPosition = transform.position;
    }
}