using UnityEngine;

public class ChestInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public InventorySO ChestLoot;

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
        ShopManager.Instance.InitializeShop(ChestLoot, "Chest", false);
    }

    public string GetInteractionText()
    {
        return "Open chest";
    }
}