using UnityEngine;
using UnityEngine.UI;

public class SearchChest : MonoBehaviour, IChestUnlockCondition
{
    [SerializeField]
    private int interactionsRequired = 5;

    [SerializeField]
    private Canvas progressCanvas;

    [SerializeField]
    private Image progressBar;

    private int _currentInteractions;

    public bool IsOpened =>
        _currentInteractions >= interactionsRequired;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.enabled = false;

        UpdateProgress();
    }

    public bool Search()
    {
        if (IsOpened)
            return true;

        _currentInteractions++;

        if (progressCanvas != null)
            progressCanvas.enabled = true;

        UpdateProgress();

        if (IsOpened)
        {
            if (progressCanvas != null)
                progressCanvas.enabled = false;

            return true;
        }

        return false;
    }

    private void UpdateProgress()
    {
        if (progressBar == null)
            return;

        progressBar.fillAmount =
            1f - ((float)_currentInteractions / interactionsRequired);
    }

    public void ShowProgress(bool value)
    {
        if (progressCanvas == null || IsOpened)
            return;

        progressCanvas.enabled = value;
    }

    public bool CanOpen()
    {
        return IsOpened;
    }

    public void Interact()
    {
        if (IsOpened)
            return;

        _currentInteractions++;

        if (progressCanvas != null)
            progressCanvas.enabled = true;

        UpdateProgress();

        if (IsOpened && progressCanvas != null)
            progressCanvas.enabled = false;
    }

    public float Progress => (float)_currentInteractions / interactionsRequired;
    
    public string GetProgressText()
    {
        return string.Empty;
    }
}