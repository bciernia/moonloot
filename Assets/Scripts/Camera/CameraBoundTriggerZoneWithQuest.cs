using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraBoundTriggerZoneWithQuest : MonoBehaviour
{
    [SerializeField] private Transform targetFocusPoint;
    [SerializeField] private GameObject upColliderGameObject;
    [SerializeField] private GameObject downColliderGameObject;
    [SerializeField] private GameObject leftColliderGameObject;
    [SerializeField] private GameObject rightColliderGameObject;
    [SerializeField] private List<EnemyStatistics> enemiesInZone;
    
    private CinemachineCamera _virtualCamera;

    private Coroutine _unblockCheckCoroutine;
    
    private QuestCompletion _questCompletion;
    
    private void Start()
    {
        _virtualCamera = FindObjectOfType<CinemachineCamera>();
        _questCompletion = GetComponent<QuestCompletion>();
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
            if (enemiesInZone.All(enemy => enemy.CurrentHP <= 0))
            {
                if (_questCompletion.Quest)
                {
                    _questCompletion.CompleteObjective();
                }

                UnblockExitFromArea();
                yield break;
            }

            yield return new WaitForSeconds(0.5f); 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    
        MoveCameraToPoint();
        BlockExitFromArea();

        if (_unblockCheckCoroutine == null)
        {
            _unblockCheckCoroutine = StartCoroutine(UnblockAreaRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    
        MoveCameraToPlayer();

        if (_unblockCheckCoroutine != null)
        {
            StopCoroutine(_unblockCheckCoroutine);
            _unblockCheckCoroutine = null;
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

    private void BlockExitFromArea() => SetCollidersAroundArea(true);
    private void UnblockExitFromArea() => SetCollidersAroundArea(false);

    private void SetCollidersAroundArea(bool shouldBeActive)
    {
        upColliderGameObject.SetActive(shouldBeActive);
        downColliderGameObject.SetActive(shouldBeActive);
        leftColliderGameObject.SetActive(shouldBeActive);
        rightColliderGameObject.SetActive(shouldBeActive);
    }
}
