using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.AI;

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
        var minItemsAmout = Mathf.Ceil(spawner.minItems * MoonManager.Instance.CurrentMoon.LootMultiplier);
        var maxItemsAmount = Mathf.Ceil((spawner.maxItems + 1) * MoonManager.Instance.CurrentMoon.LootMultiplier);
        
        var amount = Random.Range(minItemsAmout, maxItemsAmount);

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

            if (randomPos.HasValue &&
                IsPositionFree(randomPos.Value, spawner) &&
                IsFarEnough(randomPos.Value, spawnedPositions, 1.5f))
            {
                var obj = GetRandomObject();
                var go = Instantiate(obj, spawner.spawnPoint.position, Quaternion.identity);

                var mover = go.GetComponent<LootDropMover>();
                if (mover != null)
                {
                    mover.MoveToPosition(randomPos.Value);
                }
                else
                {
                    go.transform.position = randomPos.Value;
                }
                spawnedPositions.Add(randomPos.Value); 
                
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

    private Vector3? GetRandomPoint(ObjectsSpawner spawner)
    {
        var maxAttempts = 15;

        for (var i = 0; i < maxAttempts; i++)
        {
            var circle = Random.insideUnitCircle * spawner.spawnRadius;

            var rawPoint = spawner.spawnPoint.position +
                           new Vector3(circle.x, 0f, circle.y);

            if (NavMesh.SamplePosition(
                    rawPoint,
                    out NavMeshHit hit,
                    2f,
                    NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return null;
    }

    public void SpawnObjectiveItems(GameObject prefab, int amount)
    {
        var spawners = FindObjectsOfType<ObjectsSpawner>()
            .Where(x => x.isObjectiveSpawner)
            .ToList();

        if (spawners.Count == 0)
        {
            Debug.LogWarning("No objective spawners found!");
            return;
        }

        var spawnedPositions = new List<Vector3>();

        for (int i = 0; i < amount; i++)
        {
            var spawner = spawners[Random.Range(0, spawners.Count)];

            TrySpawnSpecificItem(
                prefab,
                spawner,
                spawnedPositions
            );
        }
    }
    
    private void TrySpawnSpecificItem(
        GameObject prefab,
        ObjectsSpawner spawner,
        List<Vector3> spawnedPositions)
    {
        var maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomPos = GetRandomPoint(spawner);

            if (randomPos.HasValue &&
                IsPositionFree(randomPos.Value, spawner) &&
                IsFarEnough(randomPos.Value, spawnedPositions, 1.5f))
            {
                var go = Instantiate(
                    prefab,
                    spawner.spawnPoint.position,
                    Quaternion.identity
                );

                var mover = go.GetComponent<LootDropMover>();

                if (mover != null)
                {
                    mover.MoveToPosition(randomPos.Value);
                }
                else
                {
                    go.transform.position = randomPos.Value;
                }

                spawnedPositions.Add(randomPos.Value);

                return;
            }
        }
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