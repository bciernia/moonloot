using System;
using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject selectorSprite;

    [SerializeField] private GameObject hpCanvas;

    private EnemyBrain _enemyBrain;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    private void EnemySelectedCallback(EnemyBrain enemySelected)
    {
        if (enemySelected == _enemyBrain)
        {
            SetSelectorAndHpVisibility(true);
        }
        else
        {
            SetSelectorAndHpVisibility(false);
        }
    }

    public void NoSelectionCallback()
    {
        SetSelectorAndHpVisibility(false);
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        SelectionManager.OnEnemySelectedEvent += EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent += NoSelectionCallback;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        SelectionManager.OnEnemySelectedEvent -= EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent -= NoSelectionCallback;
    }

    private void SetSelectorAndHpVisibility(bool isSelected)
    {
        selectorSprite.SetActive(isSelected);

        // if (hpCanvas != null)
        // {
            // hpCanvas.SetActive(isSelected);
        // }
    }
}