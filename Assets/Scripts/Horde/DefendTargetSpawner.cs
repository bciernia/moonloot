using UnityEngine;

public class DefendTargetSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }  
}