using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject _blocker;
    [SerializeField] private GameObject _window;

    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private Action<bool> _callback;

    private void Awake()
    {
        _yesButton.onClick.AddListener(OnYesClicked);
        _noButton.onClick.AddListener(OnNoClicked);

        HideImmediate();
    }

    public void Show(
        string description,
        Action<bool> callback)
    {
        transform.SetAsLastSibling();

        _descriptionText.text = description;
        _callback = callback;

        _blocker.SetActive(true);
        _window.SetActive(true);

        PauseManager.Instance.RequestPause();
    }

    private void OnYesClicked()
    {
        _callback?.Invoke(true);
        Close();
    }

    private void OnNoClicked()
    {
        _callback?.Invoke(false);
        Close();
    }

    private void Close()
    {
        _callback = null;

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