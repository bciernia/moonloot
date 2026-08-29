using System.Collections;
using UnityEngine;

public class FindPortalUI : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float _slideDuration = 1f;
    [SerializeField] private float _targetX = -600f;

    private RectTransform _rectTransform;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _targetPosition = _rectTransform.anchoredPosition;

        _startPosition = _targetPosition;
        _startPosition.x = _rectTransform.rect.width + 100f;

        _rectTransform.anchoredPosition = _startPosition;
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        var elapsed = 0f;

        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(elapsed / _slideDuration);

            t = 1f - Mathf.Pow(1f - t, 3f);

            _rectTransform.anchoredPosition =
                Vector2.Lerp(_startPosition, _targetPosition, t);

            yield return null;
        }

        _rectTransform.anchoredPosition = _targetPosition;
    }
}