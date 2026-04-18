using System;
using UnityEngine;

public class SellerInteraction : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] public InventorySO SellerInventory;
    [SerializeField] public string sellerId;
    
    private EnemyStatistics _enemyStatistics;
    private InventorySO _sellerInventory;
    
        
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
        _sellerInventory = Instantiate(SellerInventory);
        _sellerInventory.Initialize();
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
        ES3.Save($"shop_{sellerId}", _sellerInventory);
    }

    public void Load()
    {
        if (ES3.KeyExists($"shop_{sellerId}"))
        {
            _sellerInventory = ES3.Load<InventorySO>($"shop_{sellerId}");
        }
    }
}