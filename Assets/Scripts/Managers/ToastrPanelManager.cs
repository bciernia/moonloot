using System.Collections;
using TMPro;
using UnityEngine;

public class ToastrPanelManager : Singleton<ToastrPanelManager>
{
    [SerializeField] private GameObject toastrPanel;
    [SerializeField] private TextMeshProUGUI toastrText;

    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float animationDuration = 0.3f;

    [SerializeField] private Vector2 hiddenPosition = new(0f, 110f);
    [SerializeField] private Vector2 visiblePosition = new(0f, -10f);

    private RectTransform _panelRect;
    private Coroutine _currentCoroutine;

    protected override void Awake()
    {
        base.Awake();

        _panelRect =
            toastrPanel.GetComponent<RectTransform>();

        _panelRect.anchoredPosition =
            hiddenPosition;
    }

    public void Show(string message)
    {
        toastrText.text = message;

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine =
            StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return MovePanel(
            _panelRect.anchoredPosition,
            visiblePosition);

        yield return new WaitForSeconds(
            displayDuration);

        yield return MovePanel(
            visiblePosition,
            hiddenPosition);

        _currentCoroutine = null;
    }

    private IEnumerator MovePanel(
        Vector2 from,
        Vector2 to)
    {
        var elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            var t = Mathf.Clamp01(
                elapsed / animationDuration);

            _panelRect.anchoredPosition =
                Vector2.Lerp(from, to, t);

            yield return null;
        }

        _panelRect.anchoredPosition = to;
    }
}