using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NightOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;

    [Header("Location")]
    [SerializeField] private TextMeshProUGUI _locationName;
    [SerializeField] private Image _locationImage;

    [Header("Objective")]
    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private Image _objectiveImage;

    [Header("Mutation")]
    [SerializeField] private TextMeshProUGUI _mutationText;
    [SerializeField] private Image _mutationImage;

    [Header("Rewards")]
    [SerializeField] private Transform _rewardContainer;
    [SerializeField] private RewardElement _rewardPrefab;

    [Header("Hover")]
    [SerializeField] private float _hoverScale = 1.08f;
    [SerializeField] private float _scaleSpeed = 10f;

    [Header("Selection animation")]
    [SerializeField] private float _moveDuration = 0.4f;
    [SerializeField] private float _selectedScale = 1.1f;

    private UIManager _uiManager;
    private int _index;
    private NPCHero _hero;
    private bool _isHeroSelection;

    private Vector3 _targetScale = Vector3.one;
    private Vector3 _startPosition;

    private bool _hoverEnabled = true;

    private void Awake()
    {
        _startPosition = transform.localPosition;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            _scaleSpeed * Time.unscaledDeltaTime);
    }

    public void Setup(PreparedNightOption option, int index, UIManager uiManager)
    {
        _uiManager = uiManager;
        _index = index;
        _hero = null;
        _isHeroSelection = false;

        if (_rewardContainer != null)
        {
            _rewardContainer.gameObject.SetActive(true);
        }

        if (_objectiveImage != null)
        {
            _objectiveImage.enabled = true;
        }

        if (_mutationImage != null)
        {
            _mutationImage.enabled = true;
        }

        var location = option.Location;
        var moon = option.Moon;

        _locationName.text = location.Title;
        _locationImage.sprite = location.PreviewImage;

        if (moon != null)
        {
            _objectiveText.text = moon.ObjectiveTextLong;
            _objectiveImage.sprite = moon.Image;
        }

        _mutationText.text = option.Mutation.Description;
        _mutationImage.sprite = option.Mutation.Icon;

        SetupRewards(option);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    public void SetupForHeroSelection(
        NPCHero hero,
        int index,
        UIManager uiManager)
    {
        _uiManager = uiManager;
        _index = index;
        _hero = hero;
        _isHeroSelection = true;

        _locationName.text = hero.Name;

        var spriteRenderer = hero.Character != null
            ? hero.Character.GetComponentInChildren<SpriteRenderer>()
            : null;
        if (_locationImage != null)
        {
            _locationImage.sprite = spriteRenderer != null
                ? spriteRenderer.sprite
                : null;
        }

        if (_objectiveText != null)
        {
            _objectiveText.text = $"Strength: {hero.Strength}";
        }

        if (_objectiveImage != null)
        {
            _objectiveImage.enabled = false;
        }

        if (_mutationText != null)
        {
            _mutationText.text = $"Max HP: {hero.MaxHp}";
        }

        if (_mutationImage != null)
        {
            _mutationImage.enabled = false;
        }

        if (_rewardContainer != null)
        {
            _rewardContainer.gameObject.SetActive(false);
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_isHeroSelection)
        {
            _uiManager.SelectHero(_hero, _index);
            return;
        }

        _uiManager.SelectNight(_index);
    }

    public void PlaySelectedAnimation(
        Vector3 targetPosition,
        bool selected)
    {
        StopAllCoroutines();

        if (selected)
        {
            StartCoroutine(
                MoveToSelectedPosition(targetPosition));
        }
        else
        {
            StartCoroutine(
                MoveOut());
        }
    }

    public IEnumerator PlaySpinAndFlyOut(
        Vector3 direction,
        float distance,
        float duration,
        float spinTurns)
    {
        StopAllCoroutines();
        _hoverEnabled = false;
        _button.interactable = false;

        var startPosition = transform.localPosition;
        var targetPosition = startPosition + direction.normalized * distance;
        var startRotation = transform.localEulerAngles;
        var timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(timer / duration);
            var easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                easedT);
            transform.localEulerAngles = new Vector3(
                startRotation.x,
                startRotation.y,
                startRotation.z + 360f * spinTurns * easedT);

            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localEulerAngles = new Vector3(
            startRotation.x,
            startRotation.y,
            startRotation.z + 360f * spinTurns);
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
            var t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                t);

            yield return null;
        }

        transform.localPosition = targetPosition;
        _hoverEnabled = true;
        _button.interactable = true;
    }

    private IEnumerator MoveToSelectedPosition(
        Vector3 targetPosition)
    {
        _hoverEnabled = false;
        _button.interactable = false;

        var startPosition = transform.localPosition;
        var startScale = transform.localScale;
        var targetScale = Vector3.one * _selectedScale;

        var timer = 0f;

        while (timer < _moveDuration)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(
                timer / _moveDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t);

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t);

            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
    }

    private IEnumerator MoveOut()
    {
        _hoverEnabled = false;
        _button.interactable = false;

        var direction =
            transform.localPosition.x < 0f
                ? -1f
                : 1f;

        var targetPosition =
            transform.localPosition +
            Vector3.right * direction * 1200f;

        var startPosition = transform.localPosition;
        var timer = 0f;

        while (timer < _moveDuration)
        {
            timer += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(
                timer / _moveDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t);

            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;

        _targetScale =
            Vector3.one * _hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hoverEnabled)
            return;

        _targetScale =
            Vector3.one;
    }

    public void DisableHover()
    {
        _hoverEnabled = false;
        _targetScale = Vector3.one;
    }

    private void SetupRewards(
        PreparedNightOption option)
    {
        if (_rewardContainer == null || _rewardPrefab == null)
        {
            return;
        }

        foreach (Transform child in _rewardContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var reward in option.Rewards)
        {
            var element = Instantiate(
                _rewardPrefab,
                _rewardContainer);

            element.Setup(
                reward.Item.Image,
                reward.Amount);
        }
    }
}
