using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    public static event Action<EnemyBrain> OnEnemySelectedEvent;
    public static event Action OnNoSelectionEvent;
    
    [SerializeField] private LayerMask _enemyMask;
    //[SerializeField] private TextMeshProUGUI _enemyName;

    private Camera mainCamera;

    private void Awake()
    {
        SetMainCamera();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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
                    Debug.Log("Enemy");
                if (enemyHealth.CurrentHP <= 0)
                {
                    Debug.Log("Enemy1");
                    var enemyLoot = enemy.GetComponent<EnemyLoot>();
                    LootManager.Instance.ShowLoot(enemyLoot);
                }
                else
                {
                    Debug.Log("Enemy2");
                    OnEnemySelectedEvent?.Invoke(enemy);
                }
            }
            else
            {
                OnNoSelectionEvent?.Invoke();
            }
        }
    }
}
