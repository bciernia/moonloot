using System.Collections;
using UnityEngine;

public class DotAnimator : MonoBehaviour
{
    [SerializeField] private Sprite oneDot;
    [SerializeField] private Sprite twoDots;
    [SerializeField] private Sprite threeDots;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Sprite[] dotSprites;
    private int currentIndex = 0;
    private float interval = 0.25f;

    private Coroutine currentCoroutine;

    void Awake()
    {
        dotSprites = new Sprite[] { oneDot, twoDots, threeDots };

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null || dotSprites.Length == 0)
        {
            Debug.LogError("Brakuje SpriteRenderer lub sprite'ów!");
        }
    }

    void OnEnable()
    {
        if (spriteRenderer != null && dotSprites.Length > 0)
        {
            currentCoroutine = StartCoroutine(AnimateDots());
        }
    }

    void OnDisable()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    IEnumerator AnimateDots()
    {
        while (true)
        {
            spriteRenderer.sprite = dotSprites[currentIndex];
            currentIndex = (currentIndex + 1) % dotSprites.Length;

            yield return new WaitForSeconds(interval);
        }
    }
}