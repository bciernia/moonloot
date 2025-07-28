using System;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemsManager : Singleton<EquippedItemsManager>
{
    [SerializeField] private Image WeaponImage;
    [SerializeField] private Image RingImage;
    [SerializeField] private Image BraceletImage;
    [SerializeField] private Image NecklaceImage;
    [SerializeField] private Image ShoesImage;
    [SerializeField] private Image HelmetImage;
    [SerializeField] private Image ArmorImage;
    [SerializeField] private Image AmmunitionImage;

    [SerializeField] private EquippedItemsManagerSO EquippedItemsManagerSo;
    
    protected override void Awake()
    {
        base.Awake();
        
        SetImageSlot(WeaponImage, EquippedItemsManagerSo._weapon.Image);
        SetImageSlot(RingImage);
        SetImageSlot(BraceletImage);
        SetImageSlot(NecklaceImage);
        SetImageSlot(ShoesImage);
        SetImageSlot(HelmetImage);
        SetImageSlot(ArmorImage);
        SetImageSlot(AmmunitionImage);
    }

    public void SetItemAsEquipped(ItemSO item)
    {
        EquippedItemsManagerSo._weapon = (WeaponItemSO)item;
        SetEquippedWeaponImage(item.Image, ItemType.Weapon);
    }
    
    private void SetEquippedWeaponImage(Sprite sprite, ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                SetImageSlot(WeaponImage, sprite);
                break;
            case ItemType.Ammunition:
                AmmunitionImage.sprite = sprite;
                break;
            case ItemType.Ring:
                RingImage.sprite = sprite;
                break;
            case ItemType.Bracelet:
                BraceletImage.sprite = sprite;
                break;
            case ItemType.Necklace:
                NecklaceImage.sprite = sprite;
                break;
            case ItemType.Shoes:
                ShoesImage.sprite = sprite;
                break;
            case ItemType.Helmet:
                HelmetImage.sprite = sprite;
                break;
            case ItemType.Armor:
                ArmorImage.sprite = sprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
    }

    private void SetImageSlot(Image imageSlot, Sprite sprite = null)
    {
        if (sprite == null)
        {
            imageSlot.gameObject.SetActive(false);
            return;
        }
        
        imageSlot.gameObject.SetActive(true);
        imageSlot.sprite = sprite;
    }
}
