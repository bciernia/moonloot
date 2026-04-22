using System.Collections.Generic;
using UnityEngine;

public class LootSpawnManager : Singleton<LootSpawnManager>
{
    [SerializeField] private ObjectsDatabase _objectsDatabase;

    public void SpawnAll()
    {
        var spawners = FindObjectsOfType<ObjectsSpawner>();

        foreach (var spawner in spawners)
        {
            if (!spawner.spawnOnStart)
                continue;

            SpawnFromSpawner(spawner);
        }
    }

    private void SpawnFromSpawner(ObjectsSpawner spawner)
    {
        var amount = Random.Range(spawner.minItems, spawner.maxItems + 1);

        var spawnedPositions = new List<Vector3>();

        for (var i = 0; i < amount; i++)
        {
            TrySpawnItem(spawner, spawnedPositions);
        }
    }

    private void TrySpawnItem(ObjectsSpawner spawner, List<Vector3> spawnedPositions)
    {
        var maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomPos = GetRandomPoint(spawner);

            if (IsPositionFree(randomPos, spawner) &&
                IsFarEnough(randomPos, spawnedPositions, 1.5f))
            {
                var obj = GetRandomObject();
                var go = Instantiate(obj, spawner.spawnPoint.position, Quaternion.identity);

                var mover = go.GetComponent<LootDropMover>();
                if (mover != null)
                {
                    mover.MoveToPosition(randomPos);
                }
                else
                {
                    go.transform.position = randomPos;
                }
                spawnedPositions.Add(randomPos); 
                
                return;
            }
        }

        Debug.LogWarning("Nie znaleziono miejsca do spawnu");
    }
    
    private bool IsFarEnough(Vector3 newPos, List<Vector3> spawnedPositions, float minDistance)
    {
        foreach (var pos in spawnedPositions)
        {
            if (Vector3.Distance(pos, newPos) < minDistance)
                return false;
        }

        return true;
    }

    private Vector3 GetRandomPoint(ObjectsSpawner spawner)
    {
        var circle = Random.insideUnitCircle * spawner.spawnRadius;

        var point = spawner.spawnPoint.position + new Vector3(circle.x, circle.y, circle.y);

        return point;
    }

    private bool IsPositionFree(Vector3 position, ObjectsSpawner spawner)
    {
        return !Physics.CheckSphere(
            position,
            spawner.checkRadius,
            spawner.collisionMask
        );
    }

    private GameObject GetRandomObject()
    {
        var index = Random.Range(0, _objectsDatabase.Objects.Count);
        return _objectsDatabase.Objects[index];
    }
}