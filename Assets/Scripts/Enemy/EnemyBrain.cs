using System;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState;
    [SerializeField] private FSMState[] states ;

    private EnemyStatistics _enemyStatistics;
    
    private FSMState CurrentState { get; set; }
    public Transform Player { get; set; }
    public float AttackCooldown { get; private set; }

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
        
        CurrentState = newState;
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
}