using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillDescription : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private GameObject btnsPanel;
    [SerializeField] private GameObject lockedInfo;
    [SerializeField] private SkillsSetter skillsSetter;

    private Skill _selectedSkill;
    private TMP_Text _purchaseLabel;
    private Button _purchaseButton;
    private PlayerSkillManager _subscribedSkillManager;
    
    protected void Awake()
    {
        ConfigurePurchaseButton();
        ResetDescription();
    }

    private void OnEnable()
    {
        SubscribeToSkillChanges();
    }

    private void Start()
    {
        // Singleton Awake order can vary between scene objects.
        SubscribeToSkillChanges();
    }

    private void OnDisable()
    {
        if (_subscribedSkillManager != null)
            _subscribedSkillManager.OnSkillsChanged -= RefreshSelectedSkill;
        _subscribedSkillManager = null;
    }

    private void SubscribeToSkillChanges()
    {
        var manager = PlayerSkillManager.Instance;
        if (manager == null || _subscribedSkillManager == manager)
            return;

        if (_subscribedSkillManager != null)
            _subscribedSkillManager.OnSkillsChanged -= RefreshSelectedSkill;

        _subscribedSkillManager = manager;
        _subscribedSkillManager.OnSkillsChanged += RefreshSelectedSkill;
    }

    private void ConfigurePurchaseButton()
    {
        if (lockedInfo == null)
            return;

        _purchaseLabel = lockedInfo.GetComponentInChildren<TMP_Text>(true);
        if (_purchaseLabel == null)
            return;

        _purchaseButton = _purchaseLabel.GetComponent<Button>();
        if (_purchaseButton == null)
            _purchaseButton = _purchaseLabel.gameObject.AddComponent<Button>();

        _purchaseButton.targetGraphic = _purchaseLabel;
        _purchaseButton.transition = Selectable.Transition.None;
        _purchaseButton.onClick.RemoveListener(BuySelectedSkill);
        _purchaseButton.onClick.AddListener(BuySelectedSkill);
    }

    private void ResetDescription()
    {
        itemImage.gameObject.SetActive(false);
        title.text = "";
        description.text = "";
        btnsPanel.gameObject.SetActive(false);
        skillsSetter.Skill = null;
        _selectedSkill = null;
    }

    private void ShowButtonsIfSkillUnlocked(Skill chosenSkill)
    {
        var isUnlocked = PlayerSkillManager.Instance.IsUnlocked(chosenSkill);
        SetActionButtonsAndUnlockInfoSections(isUnlocked);
    }

    private void SetActionButtonsAndUnlockInfoSections(bool isUnlocked)
    {
        btnsPanel.SetActive(isUnlocked);
        lockedInfo.SetActive(!isUnlocked);
    }

    public void SetDescription(Skill skill)
    {
        if (skill == null)
        {
            ResetDescription();
            return;
        }

        _selectedSkill = skill;
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = skill.Icon;
        SetLocalized(title, skill.Name);
        SetLocalized(description, skill.GetDescription());
        skillsSetter.Skill = skill;
        RefreshSelectedSkill();
    }

    private void RefreshSelectedSkill()
    {
        if (_selectedSkill == null || PlayerSkillManager.Instance == null)
            return;

        var isUnlocked = PlayerSkillManager.Instance.IsUnlocked(_selectedSkill);
        ShowButtonsIfSkillUnlocked(_selectedSkill);

        if (!isUnlocked && _purchaseLabel != null)
        {
            var cost = _selectedSkill.AullaCrystalCost;
            var polishCrystalWord = GetPolishCrystalWord(cost);
            var source = $"KUP ({cost} {polishCrystalWord} Aulli)";
            var translations = new[]
            {
                source,
                $"BUY ({cost} Aulla Crystals)",
                $"ACHETER ({cost} cristaux d’Aulla)",
                $"COMPRAR ({cost} cristales de Aulla)",
                $"ACQUISTA ({cost} cristalli di Aulla)",
                $"KAUFEN ({cost} Aulla-Kristalle)",
                $"КУПИТЬ ({cost} кристаллов Ауллы)"
            };

            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.SetLocalizedText(_purchaseLabel, source, translations);
            else
                _purchaseLabel.text = source;
        }
    }

    private static string GetPolishCrystalWord(int amount)
    {
        var lastTwoDigits = amount % 100;
        var lastDigit = amount % 10;
        if (amount == 1)
            return "kryształ";

        return lastTwoDigits < 12 || lastTwoDigits > 14
               ? lastDigit >= 2 && lastDigit <= 4 ? "kryształy" : "kryształów"
               : "kryształów";
    }

    private void BuySelectedSkill()
    {
        if (_selectedSkill != null && PlayerSkillManager.Instance != null)
            PlayerSkillManager.Instance.TryPurchaseSkill(_selectedSkill);
    }

    private static void SetLocalized(TMP_Text target, string value)
    {
        if (target == null)
            return;

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLocalizedText(target, value);
        else
            target.text = value;
    }
}
