using UnityEngine;

[CreateAssetMenu]
public class EquippableItemSO : ItemSO, IDestroyableItem
{
    [field: SerializeField] 
    public ItemType ItemType { get; set; }
    public string ActionName => "Equip";
}
