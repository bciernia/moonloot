using System;
using UnityEngine;

public class SellerInteraction : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] public InventorySO SellerInventory;
    [SerializeField] public string sellerId;
    
    private EnemyStatistics _enemyStatistics;
    private InventoryRuntime _sellerInventory;
    
        
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return;

        if (string.IsNullOrEmpty(sellerId))
        {
            sellerId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
    
    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
        
        if (ES3.KeyExists($"shop_{sellerId}"))
        {
            Load();
        }
        else
        {
            _sellerInventory = new InventoryRuntime(SellerInventory);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
        }
    }
    
    public void Interact()
    {
        if (PauseManager.Instance.pauseRequests > 0) return;
        
        ShopManager.Instance.InitializeShop(_sellerInventory, _enemyStatistics.Name, InventoryType.Shop);
    }

    public string GetInteractionText()
    {
        return "Open shop";
    }
    
    public void Save()
    {
        var data = new ShopSaveData()
        {
            items = _sellerInventory.Items,
            lunar = _sellerInventory.Lunar
        };
        
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save($"shop_{sellerId}", data, settings);
    }

    public void Load()
    {
        if (_sellerInventory == null)
        {
            _sellerInventory = new InventoryRuntime(SellerInventory);
        }
        
        if (ES3.KeyExists($"shop_{sellerId}"))
        {
            var data = ES3.Load<ShopSaveData>($"shop_{sellerId}");
            _sellerInventory.LoadData(data.items, data.lunar);

            _sellerInventory.NotifyInventoryUpdated();
        }
    }
}