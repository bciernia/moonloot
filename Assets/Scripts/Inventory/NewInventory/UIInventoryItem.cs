using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image borderImage;
    [SerializeField] private GameObject quantityTextContainer;
    [SerializeField] private Sprite DefaultImage;

    public event Action<UIInventoryItem, UIInventoryItem> OnItemDroppedOn;
    
    public event Action<UIInventoryItem> OnLeftMouseBtnClick, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick;

    private bool empty = true;

    private void Awake()
    {
      //ResetData();
        Deselect();
    }

    public void ResetData()
    {
        itemImage.sprite = DefaultImage;
        
        //itemImage.gameObject.SetActive(false);
        empty = true;
    }

    public void Deselect()
    {
        borderImage.enabled = false;
    }

    public void SetData(Sprite sprite, int quantity)
    {
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        if (quantity <= 1)
        {
            quantityTextContainer.SetActive(false);
        }
        else
        {
            quantityTextContainer.SetActive(true);
            quantityText.text = quantity + "";
        }
        empty = false;
    }

    public void Select()
    {
        borderImage.enabled = true;
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseBtnClick?.Invoke(this);
        }
        else
        {
            OnLeftMouseBtnClick?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var isItemEquipped = eventData.pointerEnter.CompareTag("EquippedItem");
        if (empty && !isItemEquipped)
            return;
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var sourceUiItem = eventData.pointerDrag?.GetComponent<UIInventoryItem>();
        if (sourceUiItem == null)
            return;
        
        OnItemDroppedOn?.Invoke(this, sourceUiItem);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }
}