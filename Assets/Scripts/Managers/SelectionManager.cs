using System;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static event Action<EnemyBrain> OnEnemySelectedEvent;
    public static event Action OnNoSelectionEvent;
    
    [SerializeField] private LayerMask enemyMask;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        SelectEnemy();
    }

    private void SelectEnemy()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, enemyMask);

            if (hit.collider)
            {
                var enemy = hit.collider.GetComponent<EnemyBrain>();
                if (!enemy) return;
                var enemyHealth = enemy.GetComponent<EnemyStatistics>();
                if (enemyHealth.CurrentHP <= 0) return;
                
                OnEnemySelectedEvent?.Invoke(enemy);
            }
            else
            {
                OnNoSelectionEvent?.Invoke();
            }
        }
    }
}
