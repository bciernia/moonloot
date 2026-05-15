using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO _inventoryItem;
    [SerializeField] private ParticleSystem pickupParticles;

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
        if(!InventoryController.Instance.IsEmptySlotInEquipment())
        {
            FloatingTextManager.Instance.ShowWarningText("No empty slot in equipment", transform);
            return;
        }
        
        InventoryController.Instance.AddItem(new InventoryItem() { item = _inventoryItem, quantity = 1});
        TriggerParticles();
        Destroy(gameObject);
    }

    public string GetInteractionText() => $"Get: {_inventoryItem.Name}";

    private void TriggerParticles()
    {
        if (pickupParticles == null) return;
        var ps = Instantiate(pickupParticles, transform.position, Quaternion.identity);
        var main = ps.main;
        ps.Play();
        Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax);     
    }

    public ItemSO GetInventoryItem() => _inventoryItem;
}
