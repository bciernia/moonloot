using UnityEngine;

[CreateAssetMenu(fileName = "ItemIngredient_", menuName = "Items/Item ingredient")]
public class ItemIngredient : InventoryItem
{
    [Header("Config")]
    public float HealthValue;
    public float ManaValue;
    public float StaminaValue;

    public override bool UseItem()
    {
        switch (HealthValue)
        {
            case < 0:
                GameManager.Instance.Player.PlayerHealth.RestoreHealth(HealthValue);
                return true;
            case >= 0:
                if (GameManager.Instance.Player.PlayerHealth.CanRestoreHealth())
                    GameManager.Instance.Player.PlayerHealth.RestoreHealth(HealthValue);
                return true;                
        }
        
        return false;
    }
}