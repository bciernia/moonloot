using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class BossFightArena : Singleton<BossFightArena>
{
    private readonly float wallSpacing = 1f;
    
    private readonly List<GameObject> spawnedWalls = new();
    private Vector2Int arenaSize;

    private Coroutine _unblockCheckCoroutine;
    private GameObject arenaContainer;
    
    private List<EnemyStatistics> ArenaCreators { get; set; }
    private List<EnemyStatistics> EnemiesToSpawnInside { get; set; }
    
    private GameObject ArenaCreator { get; set; }
    private GameObject DestroyAfterArenaFinish { get; set; }

    protected void Start()
    {
        ArenaCreators = new List<EnemyStatistics>();
        EnemiesToSpawnInside = new List<EnemyStatistics>();
    }
    
    public void CreateArena(GameObject arenaCreator, GameObject wallPrefab, int arenaWidth, int arenaHeight,
        Transform centerOfArena, List<GameObject> enemiesToSpawnInside, int numberOfEnemiesToSpawn, QuestCompletion questCompletion, GameObject destroyAfterArenaFinish)
    {
        ArenaCreator = arenaCreator;
        DestroyAfterArenaFinish = destroyAfterArenaFinish;
        ArenaCreators.Add(arenaCreator.GetComponent<EnemyStatistics>());
        EnemiesToSpawnInside.ForEach(enemy => EnemiesToSpawnInside.Add(enemy));
        arenaSize = new Vector2Int(arenaWidth, arenaHeight);
        arenaContainer = new GameObject("Arena");
        arenaContainer.transform.position = transform.position;

        var center = centerOfArena.position;
        var halfX = arenaSize.x / 2f;
        var halfY = arenaSize.y / 2f;

        for (var x = -halfX; x <= halfX; x += wallSpacing)
        {
            var topPos = center + new Vector3(x, halfY, 0f);
            var bottomPos = center + new Vector3(x, -halfY, 0f);

            spawnedWalls.Add(Instantiate(wallPrefab, topPos, Quaternion.identity, arenaContainer.transform));
            spawnedWalls.Add(Instantiate(wallPrefab, bottomPos, Quaternion.identity, arenaContainer.transform));
        }

        for (var y = -halfY; y <= halfY; y += wallSpacing)
        {
            var leftPos = center + new Vector3(-halfX, y, 0f);
            var rightPos = center + new Vector3(halfX, y, 0f);

            spawnedWalls.Add(Instantiate(wallPrefab, leftPos, Quaternion.identity, arenaContainer.transform));
            spawnedWalls.Add(Instantiate(wallPrefab, rightPos, Quaternion.identity, arenaContainer.transform));
        }
        
        if (enemiesToSpawnInside.Count > 0 && numberOfEnemiesToSpawn > 0)
        {
            for (var i = 0; i < numberOfEnemiesToSpawn; i++)
            {
                var prefab = enemiesToSpawnInside[Random.Range(0, enemiesToSpawnInside.Count)];
                
                var spawnX = Random.Range(center.x - halfX + 1, center.x + halfX - 1);
                var spawnY = Random.Range(center.y - halfY + 1, center.y + halfY - 1);
                var spawnPos = new Vector3(spawnX, spawnY, 0f);
    
                var enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);
                
                var enemyStats = enemyGO.GetComponent<EnemyStatistics>();
                if (enemyStats != null)
                {
                    ArenaCreators.Add(enemyStats);
                }
            }
        }

        if (ArenaCreators != null && ArenaCreators.Count > 0 && _unblockCheckCoroutine == null)
        {
            _unblockCheckCoroutine = StartCoroutine(UnblockAreaRoutine(questCompletion));
        }
    }

    private void DestroyArena()
    {
        foreach (var wall in spawnedWalls)
        {
            if (wall != null) Destroy(wall);
        }
        spawnedWalls.Clear();

        if (arenaContainer != null)
        {
            Destroy(arenaContainer);
        }

        if (_unblockCheckCoroutine != null)
        {
            StopCoroutine(_unblockCheckCoroutine);
            _unblockCheckCoroutine = null;
        }
    }

    private IEnumerator UnblockAreaRoutine(QuestCompletion questCompletion)
    {
        while (ArenaCreators == null || ArenaCreators.Count == 0)
        {
            yield return null;
        }

        while (true)
        {
            var allDead = true;

            for (var i = 0; i < ArenaCreators.Count; i++)
            {
                var enemy = ArenaCreators[i];
                if (enemy != null && enemy.CurrentHP > 0)
                {
                    allDead = false; 
                    break;
                }
            }

            if (allDead && ArenaCreators.Count > 0)
            {
                if (questCompletion)
                {
                    questCompletion.CompleteObjective(0, "1");
                }
                
                DestroyArena();
                DestroyArenaCreator();
                DestroyObjectAfterFinishingArena();
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DestroyArenaCreator()
    {
        Destroy(ArenaCreator);
    }

    private void DestroyObjectAfterFinishingArena()
    {
        if (DestroyAfterArenaFinish) Destroy(DestroyAfterArenaFinish);
    }
}
