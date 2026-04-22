using UnityEngine;

public class ExitSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }  
}