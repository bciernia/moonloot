using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossFightArena : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallSpacing = 1f;
    [SerializeField] private int arenaWidth = 10;
    [SerializeField] private int arenaHeight = 6;

    private readonly List<GameObject> spawnedWalls = new();
    [SerializeField] private List<EnemyStatistics> _arenaCreators = new();
    private Vector2Int arenaSize;

    private Coroutine _unblockCheckCoroutine;
    private GameObject arenaContainer;
    
    private QuestCompletion _questCompletion;

    private void Awake()
    {
        arenaSize = new Vector2Int(arenaWidth, arenaHeight);
        _arenaCreators.Add(gameObject.GetComponent<EnemyStatistics>());
        _questCompletion = GetComponent<QuestCompletion>();
    }

    /// <summary>
    /// Tworzy ściany wokół obiektu (pozycja = środek areny).
    /// Uruchamia też monitoring żywotności wrogów (jeśli lista _arenaCreators nie jest pusta).
    ///
    /// Do użycia w dialogach.
    /// </summary>
    public void CreateArena()
    {
        if (wallPrefab == null) return;

        arenaContainer = new GameObject("Arena");
        arenaContainer.transform.position = transform.position;
        
        var center = transform.position;
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

        if (_arenaCreators != null && _arenaCreators.Count > 0 && _unblockCheckCoroutine == null)
        {
            _unblockCheckCoroutine = StartCoroutine(UnblockAreaRoutine());
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

    private IEnumerator UnblockAreaRoutine()
    {
        while (_arenaCreators == null || _arenaCreators.Count == 0)
        {
            yield return null;
        }

        while (true)
        {
            var allDead = true;

            for (var i = 0; i < _arenaCreators.Count; i++)
            {
                var enemy = _arenaCreators[i];
                if (enemy != null && enemy.CurrentHP > 0)
                {
                    allDead = false; 
                    break;
                }
            }

            if (allDead && _arenaCreators.Count > 0)
            {
                if (_questCompletion && _questCompletion.Quest)
                {
                    CompleteObjective("1");
                }
                
                DestroyArena();
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CompleteObjective(string objectiveToComplete)
    {
        _questCompletion.CompleteObjective(objectiveToComplete);
    }
}
