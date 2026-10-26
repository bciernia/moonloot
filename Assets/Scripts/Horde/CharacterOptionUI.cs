using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _professionText;
    [SerializeField] private Image _characterImage;
    [SerializeField] private TextMeshProUGUI _prosText;
    [SerializeField] private TextMeshProUGUI _consText;

    [Header("Hover")]
    [SerializeField] private float _hoverScale = 1.08f;
    [SerializeField] private float _scaleSpeed = 10f;

    [Header("Selection animation")]
    [SerializeField] private float _moveDuration = 0.4f;
    [SerializeField] private float _selectedScale = 1.1f;

    private UIManager _uiManager;
    private NPCHero _hero;
    private int _index;
    private bool _hoverEnabled = true;
    private Vector3 _targetScale = Vector3.one;

    public void Setup(NPCHero hero, int index, UIManager uiManager)
    {
        _hero = hero;
        _index = index;
        _uiManager = uiManager;
        _hoverEnabled = true;
        _targetScale = Vector3.one;

        _professionText.text = hero.Profession;

        if (_characterImage != null)
        {
            _characterImage.sprite = hero.Portrait;
            if (_characterImage.sprite == null && hero.Character != null)
            {
                var spriteRenderer = hero.Character
                    .GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    _characterImage.sprite = spriteRenderer.sprite;
                }
            }
        }

        _prosText.text = hero.Pros;
        _consText.text = hero.Cons;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            _scaleSpeed * Time.unscaledDeltaTime);
    }

    private void OnClick()
    {
        _uiManager.SelectHero(_hero, _index);
    }

    public void PlayEnterFromRight(float distance, float duration)
    {
        StopAllCoroutines();
        _hoverEnabled = false;
        _button.interactable = false;
        _targetScale = Vector3.one;

        var targetPosition = transform.localPosition;
        transform.localPosition = targetPosition + Vector3.right * distance;
        StartCoroutine(MoveIntoPlace(targetPosition, duration));
    }

    private IEnumerator MoveIntoPlace(Vector3 targetPosition, float duration)
    {
        var startPosition = transform.localPosition;
        var timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / duration));
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.localPosition = targetPosition;
        _hoverEnabled = true;
        _button.interactable = true;
    }

    public void PlaySelectedAnimation(Vector3 targetPosition, bool selected)
    {
        StopAllCoroutines();
        _hoverEnabled = false;
        _button.interactable = false;

        if (selected)
        {
            StartCoroutine(MoveToSelectedPosition(targetPosition));
        }
        else
        {
            StartCoroutine(MoveOut());
        }
    }

    private IEnumerator MoveToSelectedPosition(Vector3 targetPosition)
    {
        var startPosition = transform.localPosition;
        var startScale = transform.localScale;
        var targetScale = Vector3.one * _selectedScale;
        var timer = 0f;

        while (timer < _moveDuration)
        {
            timer += Time.unscaledDeltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / _moveDuration));
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
    }

    private IEnumerator MoveOut()
    {
        var direction = transform.localPosition.x < 0f ? -1f : 1f;
        var startPosition = transform.localPosition;
        var targetPosition = startPosition + Vector3.right * direction * 1200f;
        var timer = 0f;

        while (timer < _moveDuration)
        {
            timer += Time.unscaledDeltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / _moveDuration));
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverEnabled)
        {
            _targetScale = Vector3.one * _hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverEnabled)
        {
            _targetScale = Vector3.one;
        }
    }
}
