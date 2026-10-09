using UnityEngine;

public class NPCSpawnPoint : MonoBehaviour
{
    public string ID;
    public bool IsOccupied;
    public string SpawnPointName = "EMPTY_NAME";

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
