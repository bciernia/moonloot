using UnityEngine;

public class SellerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public InventorySO SellerInventory;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().SetInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().ClearInteractable();
        }
    }
    
    public void Interact()
    {
        ShopManager.Instance.InitializeShop(SellerInventory);
    }

    public string GetInteractionText()
    {
        return "Open shop";
    }
}