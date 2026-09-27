using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftFeedbackUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amountText;

    [Header("Animation")]
    [SerializeField] private float _duration = 0.8f;
    [SerializeField] private float _moveDistance = 50f;

    private RectTransform _rectTransform;
    private Vector2 _startPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startPosition = _rectTransform.anchoredPosition;
    }

    public void Show(Sprite icon, int amount)
    {
        _icon.sprite = icon;
        _amountText.text = $"+{amount}";

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        var elapsed = 0f;

        _rectTransform.anchoredPosition = _startPosition;

        var iconColor = _icon.color;
        var textColor = _amountText.color;

        iconColor.a = 1f;
        textColor.a = 1f;

        _icon.color = iconColor;
        _amountText.color = textColor;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(elapsed / _duration);

            _rectTransform.anchoredPosition =
                _startPosition + Vector2.up * (_moveDistance * t);

            var alpha = 1f - t;

            iconColor.a = alpha;
            textColor.a = alpha;

            _icon.color = iconColor;
            _amountText.color = textColor;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}