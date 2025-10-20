using System;
using UnityEngine;

using System.Collections;

public class WalkAfterAction : MonoBehaviour
{
    [SerializeField] private Transform destinationPoint;
    private bool triggered = false;

    private NPCMovement _npcMovement;

    private void Awake()
    {
        _npcMovement = GetComponent<NPCMovement>();
    }

    private void Start()
    {
        if (_npcMovement == null)
            _npcMovement = GetComponent<NPCMovement>();

        _npcMovement.OnDestinationReached += HandleDestinationReached;
    }

    public void TriggerWalk()
    {
        if (triggered) return;
        triggered = true;

        _npcMovement.MoveTo(destinationPoint.position);
    }

    private void HandleDestinationReached()
    {
        Debug.Log($"{name} dotarł do punktu {destinationPoint.name}");
        StartCoroutine(LookAtPlayer());
    }

    private IEnumerator LookAtPlayer()
    {
        yield return new WaitForSeconds(1f);
    }

    private void OnDestroy()
    {
        _npcMovement.OnDestinationReached -= HandleDestinationReached;
    }
}