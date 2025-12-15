using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraBoundTriggerZoneWithQuest : MonoBehaviour
{
    [SerializeField] private Transform targetFocusPoint;
    [SerializeField] private List<EnemyStatistics> enemiesInZone;
    [SerializeField] private string QuestObjectiveToComplete;
    [SerializeField] private bool ShouldBlockArenaOnEnter;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallSpacing = 1f;
    [SerializeField] private int arenaWidth = 10;
    [SerializeField] private int arenaHeight = 6;
    private Vector2Int arenaSize;
    
    private CinemachineCamera _virtualCamera;
    private Coroutine _unblockCheckCoroutine;
    private QuestCompletion _questCompletion;
    private readonly List<GameObject> spawnedWalls = new List<GameObject>();
    
    private void Start()
    {
        _virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        _questCompletion = GetComponent<QuestCompletion>();
        arenaSize = new Vector2Int(arenaWidth, arenaHeight);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    
        // MoveCameraToPoint();
        if (ShouldBlockArenaOnEnter)
        {
            BlockExitFromArea();
        }

        if (_unblockCheckCoroutine == null)
        {
            _unblockCheckCoroutine = StartCoroutine(UnblockAreaRoutine());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    
        // MoveCameraToPlayer();

        if (_unblockCheckCoroutine != null)
        {
            StopCoroutine(_unblockCheckCoroutine);
            _unblockCheckCoroutine = null;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var enemyStats = other.GetComponent<EnemyStatistics>();
    
        if (other.CompareTag("Enemy") && !enemiesInZone.Contains(enemyStats))
        {
            enemiesInZone.Add(enemyStats);
        }
    }
    
    private IEnumerator UnblockAreaRoutine()
    {
        while (true)
        {
            if (enemiesInZone.TrueForAll(enemy => enemy.CurrentHP <= 0))
            {
                if (_questCompletion)
                {
                    _questCompletion.CompleteObjective(0, QuestObjectiveToComplete);
                }
            
                UnblockExitFromArea();
                yield break;
            }

            yield return new WaitForSeconds(0.5f); 
        }
    }
    
    private void MoveCameraToPoint()
    {
        _virtualCamera.Target.TrackingTarget = targetFocusPoint;
    }

    private void MoveCameraToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        _virtualCamera.Target.TrackingTarget = player.transform;
    }
    
    private void BlockExitFromArea()
    {
        if (wallPrefab == null) return;

        var center = transform.position;
        var halfX = arenaSize.x / 2f;
        var halfY = arenaSize.y / 2f;

        for (var x = -halfX;x <= halfX; x += wallSpacing)
        {
            var topPos = center + new Vector3(x, halfY, 0);
            var bottomPos = center + new Vector3(x, -halfY, 0);

            spawnedWalls.Add(Instantiate(wallPrefab, topPos, Quaternion.identity, transform));
            spawnedWalls.Add(Instantiate(wallPrefab, bottomPos, Quaternion.identity, transform));
        }

        for (var y = -halfY; y <= halfY; y += wallSpacing)
        {
            var leftPos = center + new Vector3(-halfX, y, 0);
            var rightPos = center + new Vector3(halfX, y, 0);

            spawnedWalls.Add(Instantiate(wallPrefab, leftPos, Quaternion.identity, transform));
            spawnedWalls.Add(Instantiate(wallPrefab, rightPos, Quaternion.identity, transform));
        }
    }

    private void UnblockExitFromArea()
    {
        foreach (var wall in spawnedWalls)
        {
            if (wall != null) Destroy(wall);
        }
        spawnedWalls.Clear();
        Destroy(gameObject);
    }
}
