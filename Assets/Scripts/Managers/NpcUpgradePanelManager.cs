using System.Collections;
using TMPro;
using UnityEngine;

public class NPCUpgradePanelManager : Singleton<NPCUpgradePanelManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pricePanel;
    [SerializeField] private GameObject fullyUpgradedPanel;

    [Header("Rects")]
    [SerializeField] private RectTransform upgradeRect;

    [Header("Animation")]
    [SerializeField] private float animationTime = 0.75f;
    [SerializeField] private float hiddenBottomY = -1200f;

    [Header("Texts")]
    [SerializeField] private TMP_Text npcNameAndLevel;
    [SerializeField] private TMP_Text statName;
    [SerializeField] private TMP_Text statUpgrade;

    [Header("Prices")]
    [SerializeField] private GameObject priceContainer;
    [SerializeField] private GameObject pricePrefab;
    [SerializeField] private Sprite goldSprite;

    [SerializeField] private GameObject upgradeBtn;

    private NPCStatUpgrade _currentNpc;
    private Coroutine _animationRoutine;

    private void Start()
    {
        upgradeRect.anchoredPosition =
            new Vector2(0f, hiddenBottomY);

        mainPanel.SetActive(false);
    }

    public void Show(NPCStatUpgrade npc)
    {
        _currentNpc = npc;

        mainPanel.SetActive(true);

        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        _animationRoutine = StartCoroutine(ShowRoutine());

        Refresh();
    }

    public void Hide()
    {
        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        _animationRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        var startPos = new Vector2(0f, hiddenBottomY);
        var endPos = Vector2.zero;

        upgradeRect.anchoredPosition = startPos;

        var timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(timer / animationTime);
            t = Mathf.SmoothStep(0f, 1f, t);

            upgradeRect.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        upgradeRect.anchoredPosition = endPos;
    }

    private IEnumerator HideRoutine()
    {
        var startPos = upgradeRect.anchoredPosition;
        var endPos = new Vector2(0f, hiddenBottomY);

        var timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(timer / animationTime);
            t = Mathf.SmoothStep(0f, 1f, t);

            upgradeRect.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        upgradeRect.anchoredPosition = endPos;

        mainPanel.SetActive(false);

        DialogueManager.Instance.ContinueDialogue();
    }

    private void Refresh()
    {
        if (_currentNpc == null)
            return;

        foreach (Transform child in priceContainer.transform)
        {
            Destroy(child.gameObject);
        }

        var npcLevel = _currentNpc.GetLevel();
        var maxLevel = _currentNpc.GetMaxLevel();

        npcNameAndLevel.text =
            $"{_currentNpc.GetNpcProfession()} {_currentNpc.GetNpcName()}\nLevel {npcLevel} / {maxLevel}";

        if (npcLevel >= maxLevel)
        {
            upgradePanel.SetActive(false);
            pricePanel.SetActive(false);
            fullyUpgradedPanel.SetActive(true);
            upgradeBtn.SetActive(false);

            return;
        }

        upgradePanel.SetActive(true);
        pricePanel.SetActive(true);
        fullyUpgradedPanel.SetActive(false);
        upgradeBtn.SetActive(true);

        statName.text = _currentNpc.GetStatName();

        statUpgrade.text =
            $"{_currentNpc.GetCurrentBonusValue()}{GetPercentForBonuses(_currentNpc.GetNpcBonus())}" +
            $" -> " +
            $"{_currentNpc.GetNextBonusValue()}{GetPercentForBonuses(_currentNpc.GetNpcBonus())}";

        var item = _currentNpc.GetRequiredItem();

        var itemElement =
            Instantiate(pricePrefab, priceContainer.transform);

        itemElement.GetComponent<PriceElementUI>()
            .Setup(
                item.Image,
                $"{item.Name} x{_currentNpc.GetRequiredAmount()}"
            );

        var goldElement =
            Instantiate(pricePrefab, priceContainer.transform);

        goldElement.GetComponent<PriceElementUI>()
            .Setup(
                goldSprite,
                _currentNpc.GetRequiredGold().ToString()
            );
    }

    private string GetPercentForBonuses(BonusType bonus)
    {
        if (bonus == BonusType.Damage ||
            bonus == BonusType.MoveSpeed ||
            bonus == BonusType.CritChance ||
            bonus == BonusType.AttackCooldownReduction)
        {
            return "%";
        }

        return string.Empty;
    }

    public void OnUpgradeClicked()
    {
        if (_currentNpc == null)
            return;

        if (_currentNpc.TryUpgrade())
        {
            Refresh();
        }
        else
        {
            FloatingTextManager.Instance.ShowWarningText(
                "Cannot upgrade",
                transform
            );
        }
    }
}