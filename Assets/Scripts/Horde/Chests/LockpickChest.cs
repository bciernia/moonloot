using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockpickChest : MonoBehaviour, IChestUnlockCondition
{
    [Header("Lockpick")]
    [SerializeField]
    private float unlockTime = 3f;

    [SerializeField]
    [Range(0f, 1f)]
    private float progressLossOnRelease = 0.1f;

    [Header("UI")]
    [SerializeField]
    private Canvas progressCanvas;

    [SerializeField]
    private Image progressBar;

    [SerializeField]
    private TextMeshProUGUI text;

    private float _currentProgress;
    private bool _isHolding;
    
    private HordeChestInteraction _chestInteraction;

    private bool IsOpened =>
        _currentProgress >= 1f;

    private void Start()
    {
        _chestInteraction = GetComponent<HordeChestInteraction>();
        
        if (progressCanvas != null)
            progressCanvas.enabled = false;

        UpdateProgress();
    }

    private void Update()
    {
        if (!_isHolding || IsOpened)
            return;

        _currentProgress +=
            Time.deltaTime / unlockTime;
        
        _currentProgress =
            Mathf.Clamp01(_currentProgress);

        UpdateProgress();

        if (IsOpened)
        {
            _isHolding = false;

            if (progressCanvas != null)
                progressCanvas.enabled = false;
            
            _chestInteraction?.OpenChest();
        }
    }

    public bool CanOpen()
    {
        return IsOpened;
    }

    public void Interact()
    {
        if (IsOpened)
            return;

        _isHolding = true;

        if (progressCanvas != null)
            progressCanvas.enabled = true;

        UpdateProgress();
    }

    public void StopInteract()
    {
        if (!_isHolding || IsOpened)
            return;

        _isHolding = false;

        _currentProgress -= progressLossOnRelease;

        _currentProgress =
            Mathf.Clamp01(_currentProgress);

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (progressBar != null)
            progressBar.fillAmount = _currentProgress;

        if (text != null)
        {
            text.text =
                IsOpened
                    ? "Ready to open"
                    : "Crack the lock";
        }
    }

    public void ShowProgress(bool value)
    {
        if (progressCanvas == null || IsOpened)
            return;

        progressCanvas.enabled = value;

        if (value)
            UpdateProgress();
    }

    public float Progress => _currentProgress;

    public string GetProgressText()
    {
        return IsOpened
            ? "Ready to open"
            : "Crack the lock";
    }
}