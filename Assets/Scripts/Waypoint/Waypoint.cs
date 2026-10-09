using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Vector3[] points;

    public Vector3[] Points => points;
    public Vector3 EntityPosition { get; set; }

    public void SetWorldPoints(Transform[] worldPoints)
    {
        if (worldPoints == null || worldPoints.Length == 0)
            return;

        var validPointCount = 0;
        foreach (var point in worldPoints)
        {
            if (point != null)
                validPointCount++;
        }

        if (validPointCount == 0)
            return;

        EntityPosition = transform.position;
        points = new Vector3[validPointCount];

        var index = 0;
        foreach (var point in worldPoints)
        {
            if (point == null)
                continue;

            points[index++] = point.position - EntityPosition;
        }
    }

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool gameStarted;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    
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

    public Vector3 GetPosition(int pointIndex) => EntityPosition + points[pointIndex];
    public bool HasAnyWaypoints => Points.Length > 0;

}
