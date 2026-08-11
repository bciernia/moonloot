using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BloodChest : MonoBehaviour, IChestUnlockCondition
{
    [SerializeField] private int interactionsRequired = 5;
    [SerializeField] private Canvas progressCanvas;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damagePerNight = 0.2f;

    private int _currentInteractions;

    private bool IsOpened => _currentInteractions >= interactionsRequired;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.enabled = false;

        UpdateProgress();
    }

    public bool CanOpen()
    {
        return IsOpened;
    }

    public void Interact()
    {
        if (IsOpened)
            return;

        DealBloodDamage();

        _currentInteractions++;

        if (progressCanvas != null)
            progressCanvas.enabled = true;

        UpdateProgress();

        if (IsOpened && progressCanvas != null)
            progressCanvas.enabled = false;
    }

    private void DealBloodDamage()
    {
        if (Player.Instance == null)
            return;

        var currentNight = HordeManager.Instance.GetCurrentHordeNumber();

        var damageMultiplier =
            1f + (currentNight - 1) * damagePerNight;

        var damage = damageAmount * damageMultiplier;

        var playerCurrentHp = Player.Instance.PlayerHealth.CurrentHealthPoints;

        var maxDealDamage = playerCurrentHp - 1f;

        damage = Mathf.Min(damage, maxDealDamage);

        if (damage <= 0f)
            return;

        Player.Instance.PlayerHealth.TakeDamage(
            damage,
            transform,
            DamageType.Physical);
    }

    private void UpdateProgress()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount =
                (float)_currentInteractions /
                interactionsRequired;
        }

        if (text != null)
        {
            text.text =
                "Pay in blood";
        }
    }

    public void ShowProgress(bool value)
    {
        if (progressCanvas == null || IsOpened)
            return;

        progressCanvas.enabled = value;
    }

    public void StopInteract()
    {
    }

    public float Progress =>
        (float)_currentInteractions /
        interactionsRequired;

    public string GetProgressText() => "Pay in blood";
}