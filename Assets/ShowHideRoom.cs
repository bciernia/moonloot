using System.Collections;
using TMPro;
using UnityEngine;

public class ShowHideRoom : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Coroutine _fadeCoroutine;
    private TextMeshProUGUI _text;

    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeAlpha(0f));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeAlpha(1f));
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha)
    {
        var spriteStartColor = _spriteRenderer.color;
        var textStartColor = _text.color;

        var time = 0f;
        var spriteStartAlpha = spriteStartColor.a;
        var textStartAlpha = textStartColor.a;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            var t = time / fadeDuration;

            var newSpriteAlpha = Mathf.Lerp(spriteStartAlpha, targetAlpha, t);
            _spriteRenderer.color = new Color(spriteStartColor.r, spriteStartColor.g, spriteStartColor.b, newSpriteAlpha);

            var newTextAlpha = Mathf.Lerp(textStartAlpha, targetAlpha, t);
            _text.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, newTextAlpha);

            yield return null;
        }
        _spriteRenderer.color = new Color(spriteStartColor.r, spriteStartColor.g, spriteStartColor.b, targetAlpha);
        _text.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, targetAlpha);
    }
}