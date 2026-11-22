using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private TMP_Text _description;

    [SerializeField]
    private Image _image;
    private LevelUpUpgradeSO _upgrade;
    
    private Vector3 _targetScale = Vector3.one;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private bool _hoverEnabled = true;
    
    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            10f * Time.unscaledDeltaTime);
    }
    
    public void Setup(LevelUpUpgradeSO upgrade)
    {
        _upgrade = upgrade;
        var localization = LocalizationManager.Instance;
        var description = string.IsNullOrWhiteSpace(upgrade.Description)
            ? GetDefaultDescription(upgrade.Bonus)
            : upgrade.Description;
        if (localization != null)
        {
            localization.SetLocalizedText(_title, upgrade.UpgradeName);
            localization.SetLocalizedText(_description, description);
        }
        else
        {
            _title.text = upgrade.UpgradeName;
            _description.text = description;
        }
        _image.sprite = upgrade.Icon;
    }

    private static string GetDefaultDescription(StatBonus bonus)
    {
        if (bonus == null) return string.Empty;
        switch (bonus.Type)
        {
            case BonusType.Damage: return "Increase your damage.";
            case BonusType.MoveSpeed: return "Move faster across the battlefield.";
            case BonusType.CritChance: return "Increase your chance to land a critical hit.";
            case BonusType.CritMultiplier: return "Deal more damage with critical hits.";
            case BonusType.MaxHp: return "Increase your maximum health.";
            case BonusType.MaxMp: return "Increase your maximum mana.";
            case BonusType.AttackCooldownReduction: return "Reduce the time between attacks.";
            case BonusType.DamageReduction: return "Reduce the damage you receive.";
            case BonusType.HealthRegeneration: return "Restore health over time.";
            default: return string.Empty;
        }
    }

    public void OnClick()
    {
        LevelUpManager.Instance.SelectUpgrade(_upgrade);
    }
    
    public LevelUpUpgradeSO GetUpgrade()
    {
        return _upgrade;
    }
    
    public void SetInteractable(bool value)
    {
        _canvasGroup.interactable = value;
        _canvasGroup.blocksRaycasts = value;
    }
    
    public void DisableHover()
    {
        _hoverEnabled = false;
        _targetScale = Vector3.one;
    }
    
    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;

        _targetScale =
            Vector3.one * 1.08f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;
        
        _targetScale = Vector3.one;
    }
}
