using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomOwnerItemUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _itemDescription;

    [Header("Price")]
    [SerializeField] private Transform _priceContainer;
    [SerializeField] private PriceElementUI _priceElementUI;

    [Header("Craft")]
    [SerializeField] private Button _createButton;

    private CraftingRecipeSO _recipe;

    public void Setup(CraftingRecipeSO recipe)
    {
        _recipe = recipe;

        _icon.sprite = recipe.ResultItem.item.Image;
        _itemName.text = recipe.Title;
        _itemDescription.text = recipe.Description;

        SetupPrice();

        _createButton.onClick.RemoveAllListeners();
        _createButton.onClick.AddListener(OnCreateClicked);

        Refresh();
    }

    private void SetupPrice()
    {
        foreach (Transform child in _priceContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var material in _recipe.Materials)
        {
            var priceElement = Instantiate(
                _priceElementUI,
                _priceContainer);

            priceElement.Setup(
                material.Item.item.Image,
                $"{material.Item.item.Name} x{material.Amount}");
        }
    }

    public void Refresh()
    {
        if (_recipe == null)
            return;

        var canCraft =
            CraftingManager.Instance.CanCraft(_recipe);

        _createButton.interactable = canCraft;
    }

    private void OnCreateClicked()
    {
        CraftingManager.Instance.Craft(_recipe);
    }
}