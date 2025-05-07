using System;
using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject selectorSprite;

    private EnemyBrain _enemyBrain;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    private void EnemySelectedCallback(EnemyBrain enemySelected)
    {
        Debug.Log("Sel");
        if (enemySelected == _enemyBrain)
        {
            selectorSprite.SetActive(true);
        }
        else
        {
            selectorSprite.SetActive(false);
        }
    }

    public void NoSelectionCallback()
    {
        selectorSprite.SetActive(false);
    }
    
    private void OnEnable()
    {
        SelectionManager.OnEnemySelectedEvent += EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent += NoSelectionCallback;
    }

    private void OnDisable()
    {
        SelectionManager.OnEnemySelectedEvent -= EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent -= NoSelectionCallback;
    }
}