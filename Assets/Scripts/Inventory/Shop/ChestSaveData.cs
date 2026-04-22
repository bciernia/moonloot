using System;
using System.Collections.Generic;

[Serializable]
public class ChestSaveData
{
    public List<InventoryItem> items;
    public bool isOpened;
}