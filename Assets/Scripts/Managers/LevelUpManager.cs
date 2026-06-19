using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : Singleton<LevelUpManager>
{
    [SerializeField] private List<LevelUpUpgradeSO> _upgrades;

    [SerializeField] private GameObject _panel;

    [SerializeField] private Transform _cardsParent;

    [SerializeField] private LevelUpCard _cardPrefab;

    [SerializeField] private RectTransform _selectedCardPoint;

    private readonly List<LevelUpCard> _spawnedCards = new();

    private bool _selectionInProgress;

    private void OnEnable()
    {
        PlayerExp.OnLevelUp += ShowLevelUp;
    }

    private void OnDisable()
    {
        PlayerExp.OnLevelUp -= ShowLevelUp;

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ReleasePause();
        }
    }

    private void ShowLevelUp(int level)
    {
        PauseManager.Instance.RequestPause();

        ClearCards();

        var options = _upgrades
            .OrderBy(x => Random.value)
            .Take(3)
            .ToList();

        foreach (var option in options)
        {
            var card = Instantiate(
                _cardPrefab,
                _cardsParent);

            card.Setup(option);
            card.SetInteractable(false);
            _spawnedCards.Add(card);
        }

        _panel.SetActive(false);

        StartCoroutine(ShowLevelUpRoutine());
    }
    
    private IEnumerator ShowLevelUpRoutine()
    {
        yield return null;

        _panel.SetActive(true);

        yield return StartCoroutine(
            AnimateCardsIn());
    }

    private IEnumerator AnimateCardsIn()
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        for (var i = 0; i < _spawnedCards.Count; i++)
        {
            var rect =
                _spawnedCards[i]
                    .GetComponent<RectTransform>();

            var targetPosition =
                rect.anchoredPosition;

            rect.anchoredPosition =
                new Vector2(
                    targetPosition.x,
                    -1000f);

            LeanTween.move(
                    rect,
                    targetPosition,
                    0.4f)
                .setDelay(i * 0.1f)
                .setEaseOutBack()
                .setIgnoreTimeScale(true);
        }

        yield return new WaitForSecondsRealtime(0.6f);

        foreach (var card in _spawnedCards)
        {
            card.SetInteractable(true);
        }
    }

    public void SelectUpgrade(LevelUpUpgradeSO upgrade)
    {
        if (_selectionInProgress)
            return;

        _selectionInProgress = true;

        StartCoroutine(
            AnimateCardSelection(upgrade));
    }

    private IEnumerator AnimateCardSelection(
        LevelUpUpgradeSO selectedUpgrade)
    {
        LevelUpCard chosenCard = null;
        
        var layout =
            _cardsParent.GetComponent<HorizontalLayoutGroup>();
        
        layout.enabled = false;
        
        foreach (var card in _spawnedCards)
        {
            card.DisableHover();
            card.SetInteractable(false);
        }
        
        foreach (var card in _spawnedCards)
        {
            var rect =
                card.GetComponent<RectTransform>();
            card.DisableHover();

            if (card.GetUpgrade() == selectedUpgrade)
            {
                chosenCard = card;
            }
            else
            {
                var targetX =
                    rect.anchoredPosition.x < 0
                        ? -2000f
                        : 2000f;

                LeanTween.moveX(
                        rect,
                        targetX,
                        0.35f)
                    .setEaseInBack()
                    .setIgnoreTimeScale(true);

                LeanTween.scale(
                        rect,
                        Vector3.one * 0.8f,
                        0.35f)
                    .setIgnoreTimeScale(true);
            }
        }

        if (chosenCard != null)
        {
            
            var chosenCardRect = chosenCard.GetComponent<RectTransform>();
        
            chosenCard.transform.SetParent(
                _selectedCardPoint.parent,
                false);
            
            LeanTween.move(
                    chosenCard.gameObject,
                    _selectedCardPoint.position,
                    0.4f)
                .setEaseOutBack()
                .setIgnoreTimeScale(true);

            LeanTween.scale(
                    chosenCardRect,
                    Vector3.one * 1.2f,
                    0.4f)
                .setEaseOutBack()
                .setIgnoreTimeScale(true);

        }

        yield return new WaitForSecondsRealtime(0.5f);

        ApplyUpgrade(selectedUpgrade);

        ClearCards();
        layout.enabled = true;

        PauseManager.Instance.ReleasePause();

        _panel.SetActive(false);

        _selectionInProgress = false;
    }

    private void ApplyUpgrade(LevelUpUpgradeSO upgrade)
    {
        Player.Instance.PlayerStats.AddLevelBonus(
            upgrade.Bonus);

        Player.Instance.PlayerAttack.RecalculateDamage();

        Debug.Log(
            $"Selected upgrade: {upgrade.UpgradeName}");
    }

    private void ClearCards()
    {
        foreach (var card in _spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        _spawnedCards.Clear();
    }
}