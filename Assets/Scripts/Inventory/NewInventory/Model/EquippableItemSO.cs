using UnityEngine;

[CreateAssetMenu]
public class EquippableItemSO : ItemSO, IDestroyableItem
{
    public string ActionName => "Equip";
}
