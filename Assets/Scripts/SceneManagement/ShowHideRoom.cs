using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShowHideRoom : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;          // Przypisz Tilemapę w Inspectorze
    [SerializeField] private float fadeDuration = 1f;  // Czas zanikania

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponentInChildren<Tilemap>();
            if (tilemap == null)
                Debug.LogError("Brak Tilemap w obiekcie " + gameObject.name);
        }
        
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTilemap(0f));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTilemap(1f));
    }

    private IEnumerator FadeTilemap(float targetAlpha)
    {
        Color startColor = tilemap.color;
        float startAlpha = startColor.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            tilemap.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;
        }

        tilemap.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}