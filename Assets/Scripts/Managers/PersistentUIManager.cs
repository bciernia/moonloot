using UnityEngine;

public class PersistentMenuManager : Singleton<PersistentMenuManager>
{
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _loadPanel;

    public GameObject MainMenuPanel => _mainMenuPanel;
    public GameObject OptionsPanel => _optionsPanel;
    public GameObject LoadPanel => _loadPanel;
    
    public void HideAllPersistentPanels()
    {
        _mainMenuPanel.SetActive(false);
        _optionsPanel.SetActive(false);
        _loadPanel.SetActive(false);
    }
}