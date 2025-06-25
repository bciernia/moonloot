using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryItem _inventoryItem;
    [SerializeField] private ParticleSystem pickupParticles;

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
        Inventory.Instance.AddItem(_inventoryItem, 1);
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
}