using UnityEngine;

public class WorkerPoint : MonoBehaviour
{
    public WorkerJob JobType;
    public bool IsOccupied;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;

    public void ConfigurePatrol(GameObject npc)
    {
        if (npc == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        var waypoint = npc.GetComponent<Waypoint>();
        if (waypoint != null)
            waypoint.SetWorldPoints(patrolPoints);
    }
}
