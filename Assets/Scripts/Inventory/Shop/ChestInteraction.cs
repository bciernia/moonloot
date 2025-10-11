using UnityEngine;

public class ChestInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public InventorySO ChestLoot;

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
        ShopManager.Instance.InitializeShop(ChestLoot, "Chest", false);
    }

    public string GetInteractionText()
    {
        return "Open chest";
    }
}