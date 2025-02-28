using System;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState;
    [SerializeField] private FSMState[] states ;
    private FSMState CurrentState { get; set; }
    public Transform Player { get; set; }

    private void Start()
    {
        ChangeState(initState);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
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
}