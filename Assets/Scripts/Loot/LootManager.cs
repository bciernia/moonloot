using System.Security.Cryptography;
using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [Header("Config")]
    [SerializeField] private GameObject _lootPanel;
    [SerializeField] private LootButton _lootButtonPrefab;
    [SerializeField] private Transform _container;

    public void ShowLoot(EnemyLoot enemyLoot)
    {
        _lootPanel.SetActive(true);
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
}
