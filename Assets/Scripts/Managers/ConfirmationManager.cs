using System;
using UnityEngine;

public class ConfirmationManager : Singleton<ConfirmationManager>
{
    [SerializeField] private ConfirmPanelUI _confirmPanel;
    [SerializeField] private InformationPanelUI _informationPanel;

    public void ShowConfirmation(
        string description,
        Action<bool> callback)
    {
        _confirmPanel.Show(
            description,
            callback);
    }

    public void ShowInformation(string description)
    {
        _informationPanel.Show(description);
    }
}