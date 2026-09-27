using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : Singleton<CraftingManager>
{
    public event Action OnInventoryChanged;

    public bool CanCraft(CraftingRecipeSO recipe)
    {
        foreach (var material in recipe.Materials)
        {
            var amount =
                InventoryController.Instance.GetItemCount(material.Item);

            if (amount < material.Amount)
                return false;
        }

        return true;
    }

    public void Craft(CraftingRecipeSO recipe)
    {
        if (!CanCraft(recipe))
            return;

        foreach (var material in recipe.Materials)
        {
            InventoryController.Instance.RemoveItem(
                material.Item,
                material.Amount);
        }

        InventoryController.Instance.AddItem(new InventoryItem
        {
            item = recipe.ResultItem.item,
            quantity = recipe.ResultAmount,
            itemState = new List<ItemParameter>(recipe.ResultItem.item.DefaultParametersList)
        });

        OnInventoryChanged?.Invoke();
    }
}
