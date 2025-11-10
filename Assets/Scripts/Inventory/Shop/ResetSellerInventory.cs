using System;
using UnityEngine;

public class ResetSellerInventory : MonoBehaviour
{
    [SerializeField] private Transform sellerInventoryUiContainer;
    
    private void OnDisable()
    {
        // ShopManager.Instance.ResetSellerInventory();
        //
        // foreach (Transform uiItem in sellerInventoryUiContainer)
        // {
        //     Destroy(uiItem.gameObject);            
        // }
    }
}
