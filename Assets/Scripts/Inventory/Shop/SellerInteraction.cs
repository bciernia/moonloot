using UnityEngine;

public class SellerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public InventorySO SellerInventory;

    private EnemyStatistics _enemyStatistics;

    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
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
        ShopManager.Instance.InitializeShop(SellerInventory, _enemyStatistics.Name, InventoryType.Shop);
    }

    public string GetInteractionText()
    {
        return "Open shop";
    }
}