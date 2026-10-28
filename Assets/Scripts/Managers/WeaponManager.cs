using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager: Singleton<WeaponManager>
{
    [Header("Config")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _weaponManaTMP;

    [SerializeField] private WeaponItemSO _weapon;
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;

    [SerializeField] private WeaponItemSO fists;

    private Image _attackCooldownImage;

    public void SetWeapon(WeaponItemSO weaponItem, List<ItemParameter> itemState, bool isFromLoading = false)
    {
        _itemCurrentState ??= new List<ItemParameter>();
        _parametersToModify ??= new List<ItemParameter>();

        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone        
        if (weaponItem != null && _weapon != null && !isFromLoading)
        {
            _inventoryData.AddItem(_weapon, 1, _itemCurrentState);
        }
        
        _weapon = weaponItem;
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        EquipWeapon(_weapon);
    }

    private void ModifyParameters()
    {
        foreach (var parameter in _parametersToModify)
        {
            if (_itemCurrentState.Contains(parameter))
            {
                var index = _itemCurrentState.IndexOf(parameter);
                var newValue = _itemCurrentState[index].value + parameter.value;
                _itemCurrentState[index] = new ItemParameter()
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
    
    private void EquipWeapon(WeaponItemSO weapon)
    {
        if (weapon == null)
        {
            weapon = fists;
         
            //TODO wywalenie fist jako domyślnej broni 
            // Player.Instance.PlayerAttack.RecalculateDamage();
            // EquippedItemsManager.Instance.SetItemAsEquipped(null, ItemType.Weapon);
            // _weaponIcon.gameObject.SetActive(false);
            // _weaponManaTMP.gameObject.SetActive(false);
            // return;             
        }
        
        
        
        if (_weaponIcon != null)
        {
            _weaponIcon.sprite = weapon.Image;
            _weaponIcon.gameObject.SetActive(true);

            if (_attackCooldownImage != null)
                _attackCooldownImage.sprite = weapon.Image;
        }

        var projectileManaCost = weapon.ProjectilePrefab != null && weapon.ProjectilePrefab.ProjectileSo != null
            ? weapon.ProjectilePrefab.ProjectileSo.ManaCost
            : 0f;

        if (projectileManaCost != 0f)
        {
            if (_weaponManaTMP != null)
            {
                _weaponManaTMP.text = projectileManaCost.ToString(CultureInfo.InvariantCulture);
                _weaponManaTMP.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_weaponManaTMP != null)
                _weaponManaTMP.gameObject.SetActive(false);
        }

        var player = Player.Instance;
        if (player != null && player.PlayerAttack != null)
        {
            player.PlayerAttack.EquipWeapon(weapon);
            player.PlayerAttack.RecalculateDamage();
        }

        if (EquippedItemsManager.Instance != null)
            EquippedItemsManager.Instance.SetItemAsEquipped(weapon, ItemType.Weapon, 1, 0, _itemCurrentState);
    }

    public Image GetOrCreateAttackCooldownImage()
    {
        if (_attackCooldownImage != null)
            return _attackCooldownImage;

        if (_weaponIcon == null)
            return null;

        var overlayObject = new GameObject(
            "AttackCooldownOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        overlayObject.transform.SetParent(_weaponIcon.transform, false);

        var rectTransform = overlayObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        _attackCooldownImage = overlayObject.GetComponent<Image>();
        _attackCooldownImage.sprite = _weaponIcon.sprite;
        _attackCooldownImage.type = Image.Type.Filled;
        _attackCooldownImage.fillMethod = Image.FillMethod.Radial360;
        _attackCooldownImage.fillOrigin = (int)Image.Origin360.Top;
        _attackCooldownImage.fillClockwise = false;
        _attackCooldownImage.fillAmount = 0f;
        _attackCooldownImage.color = new Color(0.05f, 0.04f, 0.08f, 0.68f);
        _attackCooldownImage.raycastTarget = false;
        _attackCooldownImage.preserveAspect = true;

        return _attackCooldownImage;
    }
}
