using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }  
}
