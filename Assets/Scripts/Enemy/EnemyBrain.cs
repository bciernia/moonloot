using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState;
    [SerializeField] public FSMState[] states ;

    private EnemyStatistics _enemyStatistics;
    private bool _isRegisteredInCombat;
    
    public FSMState CurrentState { get; private set; }
    public Transform Player { get; set; }
    public float AttackCooldown { get; private set; }

    private readonly string EnemyLayerMaskAndTagName = "Enemy";
    
    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
        AttackCooldown = 0f;
    }

    private void Start()
    {
        ChangeState(initState);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
        if (AttackCooldown > 0f)
        {
            AttackCooldown -= Time.deltaTime;
        }
    }

    public void ChangeState(string newStateID)
    {
        var newState = GetState(newStateID);

        if (newState == null) 
        {
            return;
        }
        
        HandleCombatRegistration(newStateID);
        
        CurrentState = newState;
    }
    
    private void HandleCombatRegistration(string newStateID)
    {
        var shouldBeInCombat =
            newStateID == "Chase" ||
            newStateID == "Attack";

        if (shouldBeInCombat)
        {
            _isRegisteredInCombat = true;

            if (CombatManager.Instance != null)
                CombatManager.Instance.RegisterEnemy(this);
        }
        else if (_isRegisteredInCombat)
        {
            _isRegisteredInCombat = false;

            if (CombatManager.Instance != null)
                CombatManager.Instance.UnregisterEnemy(this);
        }
    }

    private FSMState GetState(string newStateID)
    {
        for (var i = 0; i < states.Length; i++)
        {
            if (states[i].ID == newStateID)
            {
                return states[i];
            }
        }

        return null;
    }

    public bool CanAttack()
    {
        return AttackCooldown <= 0f;
    }
    
    public void ResetAttackCooldown()
    {
        AttackCooldown = _enemyStatistics.TimeBetweenAttacks;
    }

    public void LetEnemyAttackPlayer()
    {
        var state = states.First(x => x.ID == "Chase");
        state.Transitions[1].TrueState = "Attack";
        SetEnemyLayer();
    }

    public void SetEnemyLayer()
    {
        var layerIndex = LayerMask.NameToLayer(EnemyLayerMaskAndTagName);
        if (layerIndex == -1)
        {
            Debug.Log("Layer not found");
            return;
        }
        gameObject.layer = layerIndex;
    }

    public void SetEnemyTag()
    {
        gameObject.tag = EnemyLayerMaskAndTagName;
    }
    
    private void OnDisable()
    {
        if (_isRegisteredInCombat && CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterEnemy(this);
            _isRegisteredInCombat = false;
        }
    }
}