using System;
using Unity.VisualScripting;
using UnityEngine;

public class ChestInteraction : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] public InventorySO ChestLoot;
    [SerializeField] public string chestId;

    private InventorySO chestLoot;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            return;

        if (string.IsNullOrEmpty(chestId))
        {
            chestId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    private void Awake()
    {
        chestLoot = ChestLoot;
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
        ShopManager.Instance.InitializeShop(chestLoot, "Chest", InventoryType.Chest);
    }

    public string GetInteractionText()
    {
        return "Open chest";
    }

    public void Save()
    {
        ES3.Save($"chest_{chestId}", ChestLoot);
    }

    public void Load()
    {
        if (ES3.KeyExists($"chest_{chestId}"))
        {
            chestLoot = ES3.Load<InventorySO>($"chest_{chestId}");
        }
    }
}