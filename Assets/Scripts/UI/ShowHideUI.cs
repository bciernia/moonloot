using System.Collections.Generic;
using UnityEngine;

public class ShowHideUI : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] GameObject uiContainer = null;
    [SerializeField] private List<GameObject> containersToDisable = new List<GameObject>();
    
    void Start()
    {
        uiContainer.SetActive(false);
        DisableAdditionalContainers();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        uiContainer.SetActive(!uiContainer.activeSelf);
        DisableAdditionalContainers();
    }

    private void DisableAdditionalContainers() => containersToDisable.ForEach(container => container.SetActive(false));
}
