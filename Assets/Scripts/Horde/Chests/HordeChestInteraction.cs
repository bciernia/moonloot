using System;
using UnityEngine;

public class HordeChestInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public string interactionText;
    
    private InventoryRuntime _chestInventory;
    private ShakeOnHit _shakeOnHit;

    private void Awake()
    {
        _shakeOnHit = GetComponent<ShakeOnHit>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
            
            GetComponent<IChestUnlockCondition>()
                ?.ShowProgress(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var condition = GetComponent<IChestUnlockCondition>();

            if (condition is LockpickChest lockpickChest)
                lockpickChest.StopInteract();

            FindFirstObjectByType<InteractionManager>()
                .UnregisterInteractable(this);

            condition?.ShowProgress(false);
        }
    }
    
    public void Interact()
    {
        var mimic = GetComponent<MimicChest>();

        if (mimic != null)
        {
            mimic.OpenChest();
            return;
        }

        var condition =
            GetComponent<IChestUnlockCondition>();

        if (condition != null)
        {
            condition.Interact();

            if (!condition.CanOpen())
            {
                _shakeOnHit?.Shake();
                return;
            }
        }

        OpenChest();
    }
    
    public void OpenChest()
    {
        var lootDropper =
            GetComponent<LootDropper>();

        lootDropper?.DropItems();

        Destroy(gameObject);
    }

    public string GetInteractionText() => string.IsNullOrEmpty(interactionText) ? "Open chest" : interactionText;
}