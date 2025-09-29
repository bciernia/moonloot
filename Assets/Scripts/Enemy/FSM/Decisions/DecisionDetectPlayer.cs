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

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyRelationship = GetComponent<EnemyRelationship>();
    }

    public override bool Decide()
    {
        return DetectPlayer();
    }

    private bool DetectPlayer()
    {
        var playerCollider = Physics2D.OverlapCircle(_enemyBrain.transform.position, range, playerMask);
        
        if (playerCollider && !_enemyRelationship.IsCharacterFriendly())
        {
            _enemyBrain.Player = playerCollider.transform;
            var directionToEnemy = (_enemyBrain.Player.position - _enemyBrain.transform.position).normalized;
            var distanceToPlayer = Vector2.Distance(_enemyBrain.transform.position, playerCollider.transform.position);

            var hit = Physics2D.Raycast(_enemyBrain.transform.position, directionToEnemy, distanceToPlayer, obstacleMask);

            if (hit.collider)
            {
                StartStopFocusTimer();
            }
            else
            {
                _stopFocusTimer = FocusTime;
            }

            if (_stopFocusTimer <= 0f)
            {
                _enemyBrain.Player = null;
                EnemyNavMeshAgent.DisableNavMeshAgent(_navMeshAgent);
                return false;
            }

            EnemyNavMeshAgent.EnableNavMeshAgent(_navMeshAgent); 

            _enemyAnimator.SetIsMoving(true);
            _enemyAnimator.SetMoveAnimation(new Vector2(directionToEnemy.x, directionToEnemy.y));
            return true;
        }

        _enemyBrain.Player = null;
        EnemyNavMeshAgent.DisableNavMeshAgent(_navMeshAgent);
        return false;
    }

    private void StartStopFocusTimer()
    {
        if (_stopFocusTimer > 0)
        {
            _stopFocusTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}