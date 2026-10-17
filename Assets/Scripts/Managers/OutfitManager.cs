using System.Collections.Generic;
using UnityEngine;

public class OutfitManager : Singleton<OutfitManager>
{
    [Header("Config")]
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private RuntimeAnimatorController _standardOutfit;
    [SerializeField] private OutfitItemSO _outfit;
    
    private PlayerAim _playerAim;
    
    private void Awake()
    {
        _playerAim = FindAnyObjectByType<PlayerAim>();
    }
    
    public void SetOutfit(OutfitItemSO outfitItem, List<ItemParameter> itemState, bool isFromLoading = false)
    {
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone, dlatego dodano flage isFromLoading
        if (outfitItem != null && _outfit != null && !isFromLoading)
        {
            _inventoryData.AddItem(_outfit, 1, _itemCurrentState);
        }
        
        _outfit = outfitItem;
        
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        ChangeOutfit(_outfit);
    }

    /// <summary>
    /// Applies a hero's outfit appearance and gameplay bonuses without equipping the item.
    /// A null outfit restores the player's standard appearance and removes outfit bonuses.
    /// </summary>
    public static void ApplyHeroOutfit(OutfitItemSO heroOutfit)
    {
        var player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning("Cannot apply hero appearance: Player was not found in the scene.");
            return;
        }

        var animator = player.GetComponent<Animator>();
        var animatorController = heroOutfit != null
            ? heroOutfit.RuntimeAnimatorController
            : Instance != null ? Instance._standardOutfit : null;

        if (animator != null && animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }
        else if (animator == null)
        {
            Debug.LogWarning("Cannot apply hero appearance: Player has no Animator component.");
        }
        else
        {
            Debug.LogWarning(heroOutfit == null
                ? "Cannot restore standard appearance: OutfitManager has no standard animator controller assigned."
                : "Cannot apply hero outfit appearance: outfit has no animator controller.");
        }

        var playerAim = player.GetComponentInChildren<PlayerAim>(true);
        if (playerAim == null)
        {
            playerAim = FindAnyObjectByType<PlayerAim>();
        }

        if (playerAim != null)
        {
            playerAim.SetOutfitAimBonus(heroOutfit != null ? heroOutfit.RangeBonus : 0f);
        }
        else
        {
            Debug.LogWarning("Cannot apply hero appearance: PlayerAim was not found.");
        }

        if (Instance != null && heroOutfit == null)
        {
            Instance._outfit = null;
            Instance._itemCurrentState = new List<ItemParameter>();
        }
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
    
    private void ChangeOutfit(OutfitItemSO outfit)
    {
        if (outfit == null)
        {
            FindAnyObjectByType<Player>().GetComponent<Animator>().runtimeAnimatorController = _standardOutfit;
            
            if (_playerAim != null) _playerAim.SetOutfitAimBonus(0f);

            return;
        }
        
        if (_playerAim != null)
            _playerAim.SetOutfitAimBonus(outfit.RangeBonus);
        FindAnyObjectByType<Player>().GetComponent<Animator>().runtimeAnimatorController = outfit.RuntimeAnimatorController;
        EquippedItemsManager.Instance.SetItemAsEquipped(outfit, ItemType.Outfit, 1, 2);
    }
}
