using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemStatistics", fileName = "ItemStats_")]
public class ItemStatsSO : ScriptableObject
{
    public string Name;
    public string Description;
    public float CurrentHP;
    public float MaxHP;
    public Effect Effect;
}
