using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Vector3[] points;

    public Vector3[] Points => points;
    public Vector3 EntityPosition { get; set; }

    private bool gameStarted;
    
    private void Start()
    {
        EntityPosition = transform.position;
        gameStarted = true;
    }

    private void OnDrawGizmos()
    {
        if (!gameStarted && transform.hasChanged)
        {
            EntityPosition = transform.position;
        }
    }

    public Vector3 GetPosition(int pointIndex)
    {
        return EntityPosition + points[pointIndex];
    }
}
