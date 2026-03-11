using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public int minEnemies = 1;
    public int maxEnemies = 3;

    public void Spawn()
    {
        int count = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < count; i++)
        {
            var prefab = enemyPrefabs[
                Random.Range(0, enemyPrefabs.Count)
            ];

            Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );
        }
    }
}