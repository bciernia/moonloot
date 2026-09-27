using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Crafting/Recipe",
    fileName = "Recipe_")]
public class CraftingRecipeSO : ScriptableObject
{
    [Header("Result")]
    public InventoryItem ResultItem;

    [Min(1)]
    public int ResultAmount = 1;

    [Header("Description")]
    public string Title;

    [TextArea(2, 4)]
    public string Description;

    [Header("Materials")]
    public List<CraftingMaterial> Materials;
}

[Serializable]
public class CraftingMaterial
{
    public InventoryItem Item;

    [Min(1)]
    public int Amount = 1;
}