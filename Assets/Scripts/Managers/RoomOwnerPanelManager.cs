using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOwnerPanelManager : Singleton<RoomOwnerPanelManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Rects")]
    [SerializeField] private RectTransform upgradeRect;

    [Header("Crafting")]
    [SerializeField] private Transform _recipeContainer;
    [SerializeField] private RoomOwnerItemUI _recipePrefab;

    [Header("Animation")]
    [SerializeField] private float animationTime = 0.75f;
    [SerializeField] private float hiddenBottomY = -1200f;

    private readonly List<RoomOwnerItemUI> _recipeItems = new();
    private Coroutine _animationRoutine;

    private void Start()
    {
        upgradeRect.anchoredPosition =
            new Vector2(0f, hiddenBottomY);

        mainPanel.SetActive(false);
        
        CraftingManager.Instance.OnInventoryChanged += RefreshRecipes;
    }
    
    private void OnDestroy()
    {
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnInventoryChanged -= RefreshRecipes;
        }
    }
    
    public void Show(RoomOwner roomOwner)
    {
        mainPanel.SetActive(true);

        SetupRecipes(roomOwner);

        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        _animationRoutine =
            StartCoroutine(ShowRoutine());
    }

    private void SetupRecipes(RoomOwner roomOwner)
    {
        foreach (Transform child in _recipeContainer)
        {
            Destroy(child.gameObject);
        }

        _recipeItems.Clear();

        foreach (var recipe in roomOwner.Recipes)
        {
            var itemUI = Instantiate(
                _recipePrefab,
                _recipeContainer);

            itemUI.Setup(recipe);

            _recipeItems.Add(itemUI);
        }
    }
    
    private void RefreshRecipes()
    {
        foreach (var recipeItem in _recipeItems)
        {
            if (recipeItem != null)
            {
                recipeItem.Refresh();
            }
        }
    }

    private IEnumerator ShowRoutine()
    {
        var startPos =
            new Vector2(0f, hiddenBottomY);

        var endPos = Vector2.zero;

        upgradeRect.anchoredPosition = startPos;

        var timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(
                timer / animationTime);

            t = Mathf.SmoothStep(0f, 1f, t);

            upgradeRect.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        upgradeRect.anchoredPosition = endPos;
    }

    public void Hide()
    {
        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        _animationRoutine =
            StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        var startPos = upgradeRect.anchoredPosition;
        var endPos =
            new Vector2(0f, hiddenBottomY);

        var timer = 0f;

        while (timer < animationTime)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(
                timer / animationTime);

            t = Mathf.SmoothStep(0f, 1f, t);

            upgradeRect.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        upgradeRect.anchoredPosition = endPos;

        mainPanel.SetActive(false);

        DialogueManager.Instance.ContinueDialogue();
    }
}