using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LootButton : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemQuantity;

    public DropItem ItemLoaded { get; private set; }

    public void ConfigLootButton(DropItem dropItem)
    {
        ItemLoaded = dropItem;
        _itemIcon.sprite = dropItem.Item.item.Image;
        _itemName.text = dropItem.Item.item.Name;
        _itemQuantity.text = $"x{dropItem.Quantity.ToString()}";
    }

    public void CollectItem()
    {
        if (ItemLoaded == null) return;
        InventoryController.Instance.AddItem(new InventoryItem(){item = ItemLoaded.Item.item, quantity = ItemLoaded.Quantity});
        ItemLoaded.PickedItem = true;
        Destroy(gameObject);
    }
}
