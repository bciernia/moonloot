using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowHideRoom : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Coroutine _fadeCoroutine;
    private BoxCollider2D _collider;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Transform spriteVisual;
    [SerializeField] public TextMeshPro _text;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        FitSpriteToCollider();
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

    private void FitSpriteToCollider()
    {
        if (_spriteRenderer.sprite == null) return;

        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
        Vector2 colliderSize = _collider.size;

        Vector3 newScale = new Vector3(
            colliderSize.x / spriteSize.x,
            colliderSize.y / spriteSize.y,
            1f
        );

        spriteVisual.localScale = newScale;
    }
}