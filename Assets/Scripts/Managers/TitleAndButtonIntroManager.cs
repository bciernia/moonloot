using UnityEngine;
using System.Collections;
using TMPro;

public class TitleAndButtonsIntro2D : MonoBehaviour
{
    [Header("References")]
    public Transform title;                
    public SpriteRenderer titleRenderer;   
    public CanvasGroup buttonsCanvasGroup; 
    public RectTransform buttonsPanel;     

    [Header("Animation Settings")]
    public float titleFadeDuration = 1f;      
    public float titleHoldTime = 3f;          
    public float moveDuration = 1.5f;         
    public Vector3 titleEndOffset = new Vector3(0, 3f, 0); 
    public float buttonsMoveDistance = 500f; 

    private Vector3 titleStartPos;
    private Vector3 titleEndPos;
    private Vector2 buttonsStartPos;
    private Vector2 buttonsEndPos;

    [SerializeField] private TextMeshProUGUI version;

    void Start()
    {
        version.text = $"v{Application.version}";
        
        titleStartPos = title.position;
        titleEndPos = titleStartPos + titleEndOffset;

        buttonsEndPos = buttonsPanel.anchoredPosition;
        buttonsStartPos = new Vector2(buttonsEndPos.x, buttonsEndPos.y - buttonsMoveDistance);
        buttonsPanel.anchoredPosition = buttonsStartPos;
        buttonsCanvasGroup.alpha = 0f;

        if (titleRenderer != null)
        {
            Color c = titleRenderer.color;
            c.a = 0f;
            titleRenderer.color = c;
        }

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        var t = 0f;
        while (t < titleFadeDuration)
        {
            var lerp = t / titleFadeDuration;
            SetTitleAlpha(Mathf.Lerp(0f, 1f, lerp));
            t += Time.deltaTime;
            yield return null;
        }
        SetTitleAlpha(1f);

        yield return new WaitForSeconds(titleHoldTime);

        StartCoroutine(MoveButtons());
        t = 0f;
        while (t < moveDuration)
        {
            var lerp = t / moveDuration;
            title.position = Vector3.Lerp(titleStartPos, titleEndPos, SmoothStep(lerp));
            t += Time.deltaTime;
            yield return null;
        }
        title.position = titleEndPos;
    }

    IEnumerator MoveButtons()
    {
        var t = 0f;
        while (t < moveDuration)
        {
            var lerp = t / moveDuration;
            buttonsPanel.anchoredPosition = Vector2.Lerp(buttonsStartPos, buttonsEndPos, SmoothStep(lerp));
            buttonsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, lerp);
            t += Time.deltaTime;
            yield return null;
        }
        buttonsPanel.anchoredPosition = buttonsEndPos;
        buttonsCanvasGroup.alpha = 1f;
    }

    void SetTitleAlpha(float alpha)
    {
        if (titleRenderer == null) return;
        Color c = titleRenderer.color;
        c.a = alpha;
        titleRenderer.color = c;
    }

    float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
