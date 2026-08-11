using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyChest : MonoBehaviour, IChestUnlockCondition
{
    [Header("Key")]
    [SerializeField]
    private InventoryItem requiredKey;

    [SerializeField]
    private bool consumeKey = true;

    [Header("UI")]
    [SerializeField]
    private Canvas progressCanvas;

    [SerializeField]
    private TextMeshProUGUI progressText;

    [SerializeField]
    private Image keyImage;

    private bool _unlocked;
    
    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.enabled = false;

        if (keyImage != null)
            keyImage.sprite = requiredKey.item.Image;
    }

    public bool CanOpen()
    {
        return _unlocked;
    }

    public void Interact()
    {
        if (_unlocked)
            return;

        if (InventoryController.Instance.GetItemCount(requiredKey) <= 0)
        {
            UpdateUI();
            return;
        }

        if (consumeKey)
        {
            InventoryController.Instance.RemoveItem(requiredKey, 1);
        }

        _unlocked = true;

        UpdateUI();
    }

    private bool HasUserKey() => InventoryController.Instance.GetItemCount(requiredKey) > 0;

    public float Progress => CanOpen() ? 1f : 0f;

    public string GetProgressText()
    {
        return HasUserKey()
            ? "Ready to open"
            : "Need key";
    }

    public void ShowProgress(bool value)
    {
        if (progressCanvas == null)
            return;

        progressCanvas.enabled = value;

        if (value)
            UpdateUI();
    }

    public void StopInteract()
    {
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text =
                HasUserKey()
                    ? "Ready to open"
                    : "Need key";
        }

        if (keyImage != null)
        {
            keyImage.enabled = !HasUserKey();

            keyImage.sprite = requiredKey.item.Image;
        }
    }
}