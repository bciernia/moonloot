using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [Header("Config")]
    [SerializeField] private GameObject _lootPanel;
    [SerializeField] private LootButton _lootButtonPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private TextMeshProUGUI _enemyName;

    
    public void ShowLoot(string enemyName, EnemyLoot enemyLoot)
    {
        if (IsLootEmpty(enemyLoot.Items))
        {
            //TODO character has to say that this enemy is empty
            Debug.Log("No items in loot");

            return;
        }
        
        _lootPanel.SetActive(true);
        _enemyName.text = enemyName;
        
        if (LootPanelWithItems())
        {
            for (var i = 0; i < _container.childCount; i++)
            {
                Destroy(_container.GetChild(i).gameObject);
            }
        }

        foreach (var item in enemyLoot.Items)
        {
            if (item.PickedItem) continue;

            var lootButton = Instantiate(_lootButtonPrefab, _container);
            lootButton.ConfigLootButton(item);
        }
    }

    public void ClosePanel()
    {
        _lootPanel.SetActive(false);
    }
    
    private bool LootPanelWithItems()
    {
        return _container.childCount > 0;
    }

    private bool IsLootEmpty(List<DropItem> items) => items.All(item => item.PickedItem);
}
