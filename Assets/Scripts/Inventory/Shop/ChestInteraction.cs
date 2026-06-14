using System;
using UnityEngine;

public class ChestInteraction : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] public InventorySO ChestLoot;
    [SerializeField] public string chestId;
    [SerializeField] public string interactionText;

    private InventoryRuntime _chestInventory;
    
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
        _chestInventory = new InventoryRuntime(ChestLoot);

        Load();
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
        var mimic = GetComponent<MimicChest>();

        if (mimic != null)
        {
            mimic.OpenChest();
            return;
        }
        
        if (PauseManager.Instance.pauseRequests > 0) return;
        
        ShopManager.Instance.InitializeShop(_chestInventory, "Chest", InventoryType.Chest);
    }

    public string GetInteractionText() => string.IsNullOrEmpty(interactionText) ? "Open chest" : interactionText;

    public InventoryRuntime GetRuntimeInventory() => _chestInventory;

    public void Save()
    {
        var data = new ChestSaveData()
        {
            items = _chestInventory.Items,
            isOpened = _chestInventory.Items.TrueForAll(i => i.IsEmpty)
        };

        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save($"chest_{chestId}", data, settings);
    }

    public void Load()
    {
        if (_chestInventory == null)
            _chestInventory = new InventoryRuntime(ChestLoot);

        if (ES3.KeyExists($"chest_{chestId}"))
        {
            var data = ES3.Load<ChestSaveData>($"chest_{chestId}");

            _chestInventory.LoadData(data.items, 0); // chest raczej nie ma waluty
        }
    }
}