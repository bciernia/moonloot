using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    [Header("Spawn Settings")]
    public int minItems = 1;
    public int maxItems = 3;
    public bool spawnOnStart = true;
    
    [Header("Spawn Area")]
    public float spawnRadius = 3f;

    [Header("Collision")]
    public float checkRadius = 0.5f;
    public LayerMask collisionMask;
    
    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }
}