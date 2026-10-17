using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenuUI : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private NPCHero hero;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private bool isLocked;

    public NPCHero Hero => hero;
    public bool IsLocked => isLocked || hero == null;
    public event Action<CharacterMenuUI, NPCHero> OnCharacterSelected;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(SelectCharacter);
            selectButton.onClick.AddListener(SelectCharacter);
        }

        SetSelected(false);
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(SelectCharacter);

    }

    public void Setup(NPCHero character)
    {
        hero = character;
        if (characterImage == null && selectButton != null)
            characterImage = selectButton.targetGraphic as Image;

        if (characterImage == null)
            characterImage = GetComponentInChildren<Image>(true);

        RefreshCard();
    }

    public void SetPortrait(Sprite sprite)
    {
        if (sprite == null || IsLocked)
            return;

        if (characterImage == null && selectButton != null)
            characterImage = selectButton.targetGraphic as Image;
        if (characterImage == null)
            characterImage = GetComponentInChildren<Image>(true);

        if (characterImage != null)
        {
            characterImage.sprite = sprite;
            characterImage.enabled = true;
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        RefreshCard();
    }

    public void SelectCharacter()
    {
        if (hero == null)
            return;

        OnCharacterSelected?.Invoke(this, hero);
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected && !IsLocked;
    }

    private void RefreshCard()
    {
        var locked = IsLocked;
        if (characterImage != null)
        {
            if (locked)
            {
                characterImage.sprite = lockedSprite;
                characterImage.enabled = lockedSprite != null;
            }
            else
            {
                characterImage.sprite = hero.Portrait;
                if (characterImage.sprite == null && hero.Character != null)
                {
                    var spriteRenderer = hero.Character.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null)
                        characterImage.sprite = spriteRenderer.sprite;
                }

                characterImage.enabled = characterImage.sprite != null;
            }
        }

        if (selectButton != null)
            selectButton.interactable = !locked && hero.Character != null;

        SetSelected(false);
    }

}
