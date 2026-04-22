using UnityEngine;
using UnityEngine.AI;

public class DecisionDetectPlayer : FSMDecision
{
    [SerializeField] public float range;
    [SerializeField] public LayerMask playerMask;
    [SerializeField] public LayerMask obstacleMask;
    [SerializeField] public float FocusTime = 3f;

    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private NavMeshAgent _navMeshAgent;
    private float _stopFocusTimer;
    private EnemyRelationship _enemyRelationship;
    private EnemyStatistics _enemyStatistics;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyRelationship = GetComponent<EnemyRelationship>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override bool Decide()
    {
        return DetectPlayer();
    }

private bool DetectPlayer()
{
    if (_enemyBrain.HasForcedTarget)
    {
        var target = _enemyBrain.Player;

        if (target == null)
            return false;

        var direction = (target.position - transform.position).normalized;

        EnemyNavMeshAgent.EnableNavMeshAgent(_navMeshAgent);

        _enemyAnimator.SetIsMoving(true);
        _enemyAnimator.SetMoveAnimation(new Vector2(direction.x, direction.y));

        return true;
    }
    
    Transform player = null;
    Transform defendTarget = null;

    var playerCollider = Physics2D.OverlapCircle(
        _enemyBrain.transform.position,
        _enemyStatistics.DetectRange,
        playerMask);

    if (playerCollider)
        player = playerCollider.transform;

    if (HordeManager.Instance != null &&
        HordeManager.Instance.CurrentObjective == HordeObjective.DefendObject)
    {
        defendTarget = HordeManager.Instance.DefendTarget;
    }

    Transform chosenTarget = null;

    if (player != null && defendTarget != null)
    {
        var distPlayer = Vector2.Distance(transform.position, player.position);
        var distDefend = Vector2.Distance(transform.position, defendTarget.position);

        chosenTarget = distPlayer < distDefend * 0.8f ? player : defendTarget;
    }
    else if (player != null)
    {
        chosenTarget = player;
    }
    else if (defendTarget != null)
    {
        chosenTarget = defendTarget;
    }

    if (chosenTarget == null || _enemyRelationship.IsCharacterFriendly())
    {
        _enemyBrain.Player = null;
        EnemyNavMeshAgent.DisableNavMeshAgent(_navMeshAgent);
        return false;
    }

    _enemyBrain.Player = chosenTarget;

    var directionToTarget = (_enemyBrain.Player.position - _enemyBrain.transform.position).normalized;
    var distanceToTarget = Vector2.Distance(_enemyBrain.transform.position, _enemyBrain.Player.position);

    //TODO Przestaje gonić gracza jeśli go nie widzi
    // if (chosenTarget == player)
    // {
    //     var hit = Physics2D.Raycast(
    //         _enemyBrain.transform.position,
    //         directionToTarget,
    //         distanceToTarget,
    //         obstacleMask);
    //
    //     if (hit.collider)
    //     {
    //         StartStopFocusTimer();
    //     }
    //     else
    //     {
    //         _stopFocusTimer = FocusTime;
    //     }
    //
    //     if (_stopFocusTimer <= 0f)
    //     {
    //         _enemyBrain.Player = null;
    //         EnemyNavMeshAgent.DisableNavMeshAgent(_navMeshAgent);
    //         return false;
    //     }
    // }
    // else
    // {
    //     _stopFocusTimer = FocusTime;
    // }

    EnemyNavMeshAgent.EnableNavMeshAgent(_navMeshAgent);

    _enemyAnimator.SetIsMoving(true);
    _enemyAnimator.SetMoveAnimation(new Vector2(directionToTarget.x, directionToTarget.y));

    return true;
}

    private void StartStopFocusTimer()
    {
        if (_stopFocusTimer > 0)
        {
            _stopFocusTimer -= Time.deltaTime;
        }
    }

    // private void OnDrawGizmos()
    // {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, range);
    // }
}