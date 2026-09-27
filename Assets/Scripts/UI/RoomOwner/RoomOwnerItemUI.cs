using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomOwnerItemUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _itemDescription;
    
    [Header("Feedback")]
    [SerializeField] private CraftFeedbackUI _craftFeedbackUI;

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
            
            var ownedAmount = InventoryController.Instance.GetItemCount(material.Item);
            
            priceElement.Setup(
                material.Item.item.Image,
                material.Item.item.Name,
                material.Amount,
                ownedAmount);
        }
    }

    public void Refresh()
    {
        if (_recipe == null)
            return;

        SetupPrice();
        
        var canCraft =
            CraftingManager.Instance.CanCraft(_recipe);

        _createButton.interactable = canCraft;
    }

    private void OnCreateClicked()
    {
        if (CraftingManager.Instance.Craft(_recipe))
        {
            _craftFeedbackUI.Show(_recipe.ResultItem.item.Image, _recipe.ResultAmount);
        }
    }
}