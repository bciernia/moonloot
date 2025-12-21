using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    public static event Action<EnemyBrain> OnEnemySelectedEvent;
    public static event Action OnNoSelectionEvent;
    
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private LayerMask _npcMask;
    [SerializeField] private GameObject _npcInfoManager;

    private Camera mainCamera;

    private void Awake()
    {
        SetMainCamera();
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMainCamera();
    }

    private void SetMainCamera() => mainCamera = Camera.main;

    private void Update()
    {
        SelectEnemy();
    }

    private void SelectEnemy()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, _enemyMask);

            if (hit.collider)
            {
                var enemy = hit.collider.GetComponent<EnemyBrain>();
                if (!enemy) return;
                var enemyHealth = enemy.GetComponent<EnemyStatistics>();

                if (enemyHealth.CurrentHP <= 0)
                {
                    var enemyLoot = enemy.GetComponent<EnemyLoot>();

                    NPCInfoManager.Instance?.HideNpcInfo();
                    _npcInfoManager.SetActive(false);
                }
                else
                {
                    OnEnemySelectedEvent?.Invoke(enemy);
                    _npcInfoManager.SetActive(true);
                    NPCInfoManager.Instance.ShowNpcInfo(enemyHealth);
                }
            }
            else
            {
                OnNoSelectionEvent?.Invoke();

                if (!_npcInfoManager.activeSelf) return;
                
                _npcInfoManager.SetActive(false);
                NPCInfoManager.Instance.HideNpcInfo();
            }
        }
    }
}
