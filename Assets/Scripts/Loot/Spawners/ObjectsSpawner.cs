using UnityEngine;

public class ObjectsSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public BoxCollider2D spawnArea { get; private set; }
    
    [Header("Spawn Settings")]
    public int minItems = 0;
    public int maxItems = 2;
    public bool spawnOnStart = true;
    
    [Header("Spawn Area")]
    public float spawnRadius = 3f;

    [Header("Collision")]
    public float checkRadius = 0.5f;
    public LayerMask collisionMask;

    public bool isObjectiveSpawner;    
    
    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
        
        spawnArea = GetComponent<BoxCollider2D>();
    }
}