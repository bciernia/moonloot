using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject _blocker;
    [SerializeField] private GameObject _window;

    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _okButton;

    private void Awake()
    {
        _okButton.onClick.AddListener(Close);

        HideImmediate();
    }

    public void Show(string description)
    {
        transform.SetAsLastSibling();

        _descriptionText.text = description;

        _blocker.SetActive(true);
        _window.SetActive(true);

        PauseManager.Instance.RequestPause();
    }

    private void Close()
    {
        _blocker.SetActive(false);
        _window.SetActive(false);

        PauseManager.Instance.ReleasePause();
    }

    private void HideImmediate()
    {
        _blocker.SetActive(false);
        _window.SetActive(false);
    }
}