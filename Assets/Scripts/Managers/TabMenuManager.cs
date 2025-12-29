using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TabMenuManager : Singleton<TabMenuManager>
{
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject[] Tabs;
    [SerializeField] private Image[] TabButtons;
    [SerializeField] private Sprite InactiveTabBG;
    [SerializeField] private Sprite ActiveTabBG;
    [SerializeField] private Vector2 InactiveTabBtnSize;
    [SerializeField] private Vector2 ActiveTabBtnSize;
    
    public void SwitchToTab(int tabId)
    {
        foreach (var tab in Tabs)
        {
            tab.SetActive(false);
        }
            
        Tabs[tabId].SetActive(true);
        
        foreach (var img in TabButtons)
        {
            img.sprite = InactiveTabBG;
            img.rectTransform.sizeDelta = InactiveTabBtnSize;
        }
        
        TabButtons[tabId].sprite = ActiveTabBG;
        TabButtons[tabId].rectTransform.sizeDelta = ActiveTabBtnSize;
    }

    public void OpenMenu()
    {
        MenuPanel.SetActive(true);
    }
}