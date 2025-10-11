using System;
using UnityEngine;

public class ChestLoot : MonoBehaviour, IInteractable
{
    private EnemyLoot _loot;
    
    private void Awake()
    {
        _loot = gameObject.GetComponent<EnemyLoot>();
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
        LootManager.Instance.ShowLoot("Chest", _loot);
    }

    public string GetInteractionText() => "Open chest";
}
