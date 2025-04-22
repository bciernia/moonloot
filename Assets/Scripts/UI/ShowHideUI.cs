using UnityEngine;

public class ShowHideUI : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] GameObject uiContainer = null;

    // Start is called before the firs frame update
    void Start()
    {
        uiContainer.SetActive(false);
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
    }
}
