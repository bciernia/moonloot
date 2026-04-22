using UnityEngine;

public class NpcSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }  
}
