using TMPro;
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
        _itemIcon.sprite = dropItem.Item.Icon;
        _itemName.text = dropItem.Item.Name;
        _itemQuantity.text = $"x{dropItem.Quantity.ToString()}";
    }

    public void CollectItem()
    {
        if (ItemLoaded == null) return;
        Inventory.Instance.AddItem(ItemLoaded.Item, ItemLoaded.Quantity);
        ItemLoaded.PickedItem = true;
        Destroy(gameObject);
    }
}
