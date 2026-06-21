using System;
using UnityEngine;

public class ConfirmationManager : Singleton<ConfirmationManager>
{
    [SerializeField] private ConfirmPanelUI _confirmPanel;

    public void ShowConfirmation(
        string description,
        Action<bool> callback)
    {
        _confirmPanel.Show(
            description,
            callback);
    }
}