using UnityEngine;
using System.Collections;
using TMPro;

public class TitleAndButtonsIntro2D : MonoBehaviour
{
    [Header("References")]
    public Transform title;
    public SpriteRenderer titleRenderer;

    [Header("Buttons")]
    public CanvasGroup buttonsCanvasGroup;
    public RectTransform buttonsPanel;

    [Header("Credits")]
    public Transform creditsCameraPoint;
    public CanvasGroup creditsCanvasGroup;
    [SerializeField] private GameObject discordButton;
    [SerializeField] private CanvasGroup discordCanvasGroup;
    [SerializeField, Min(0.1f)] private float discordFadeDuration = 1.2f;
    public Vector3 creditsTitleOffset = new(0, .5f, 0);

    [Header("Camera")]
    public Camera menuCamera;

    [Header("Animation")]
    public float titleFadeDuration = 1f;
    public float titleHoldTime = 3f;
    public float moveDuration = 1.5f;

    public Vector3 titleEndOffset = new(0,3,0);
    public float buttonsMoveDistance = 500f;
    [Header("Character selection")]
    public Vector3 characterSelectionTitleOffset = new(0, 2, 0);
    [SerializeField, Min(0.01f)] private float characterSelectionTitleMoveDuration = 0.9f;


    private Vector3 cameraStartPos;

    private Vector3 titleStartPos;
    private Vector3 titleEndPos;
    private Vector3 titleCreditsPos;


    private Vector2 buttonsStartPos;
    private Vector2 buttonsEndPos;


    private bool isShowingCredits;
    private bool isAnimating;
    private Coroutine characterSelectionTitleRoutine;


    [SerializeField]
    private TextMeshProUGUI version;



    void Start()
    {
        version.text = $"v{Application.version}";

        if (discordCanvasGroup == null && discordButton != null)
        {
            discordCanvasGroup = discordButton.GetComponent<CanvasGroup>();
            if (discordCanvasGroup == null)
                discordCanvasGroup = discordButton.AddComponent<CanvasGroup>();
        }

        cameraStartPos = menuCamera.transform.position;


        titleStartPos = title.position;
        titleEndPos = titleStartPos + titleEndOffset;


        buttonsEndPos = buttonsPanel.anchoredPosition;

        buttonsStartPos =
            buttonsEndPos -
            new Vector2(0, buttonsMoveDistance);


        buttonsPanel.anchoredPosition = buttonsStartPos;
        buttonsCanvasGroup.alpha = 0;


        creditsCanvasGroup.alpha = 0;
        creditsCanvasGroup.gameObject.SetActive(false);

        if (discordCanvasGroup != null)
        {
            discordCanvasGroup.alpha = 0;
            discordCanvasGroup.interactable = false;
            discordCanvasGroup.blocksRaycasts = false;
        }

        if(titleRenderer)
        {
            var c = titleRenderer.color;
            c.a = 0;
            titleRenderer.color = c;
        }


        StartCoroutine(PlayIntro());
    }



    void Update()
    {
        if(isShowingCredits && Input.GetKeyDown(KeyCode.Escape))
            CloseCredits();
    }

    public void SetCharacterSelectionTitle(bool isSelectingCharacter)
    {
        if (title == null || isShowingCredits)
            return;

        if (characterSelectionTitleRoutine != null)
            StopCoroutine(characterSelectionTitleRoutine);

        var targetPosition = isSelectingCharacter
            ? titleEndPos + characterSelectionTitleOffset * 0.5f
            : titleEndPos;
        characterSelectionTitleRoutine = StartCoroutine(MoveCharacterSelectionTitle(targetPosition));
    }

    IEnumerator MoveCharacterSelectionTitle(Vector3 targetPosition)
    {
        var startPosition = title.position;
        var duration = Mathf.Max(0.01f, characterSelectionTitleMoveDuration);
        var elapsed = 0f;
        while (elapsed < duration)
        {
            var lerp = SmoothStep(elapsed / duration);
            title.position = Vector3.Lerp(startPosition, targetPosition, lerp);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        title.position = targetPosition;
        characterSelectionTitleRoutine = null;
    }



    public void OpenCredits()
    {
        if (isAnimating || isShowingCredits) return;

        StartCoroutine(OpenCreditsRoutine());
    }



    IEnumerator OpenCreditsRoutine()
    {
        isAnimating = true;
        isShowingCredits = true;
        if (discordCanvasGroup != null)
            StartCoroutine(FadeDiscord(0f));


        creditsCanvasGroup.gameObject.SetActive(true);


        Vector3 camFrom = menuCamera.transform.position;
        Vector3 camTo = creditsCameraPoint.position;


        Vector3 titleFrom = title.position;

        titleCreditsPos =
            titleEndPos +
            (camTo - cameraStartPos) +
            creditsTitleOffset;


        float t = 0;


        while(t < moveDuration)
        {
            float lerp = SmoothStep(t / moveDuration);


            menuCamera.transform.position =
                Vector3.Lerp(camFrom, camTo, lerp);


            title.position =
                Vector3.Lerp(titleFrom, titleCreditsPos, lerp);


            buttonsPanel.anchoredPosition =
                Vector2.Lerp(buttonsEndPos, buttonsStartPos, lerp);


            buttonsCanvasGroup.alpha =
                Mathf.Lerp(1,0,lerp);


            creditsCanvasGroup.alpha =
                Mathf.Lerp(0,1,lerp);


            t += Time.deltaTime;
            yield return null;
        }


        isAnimating = false;
    }



    void CloseCredits()
    {
        if(isAnimating) return;

        StartCoroutine(CloseCreditsRoutine());
    }



    IEnumerator CloseCreditsRoutine()
    {
        isAnimating = true;


        Vector3 camFrom = menuCamera.transform.position;
        Vector3 titleFrom = title.position;


        float t = 0;


        while(t < moveDuration)
        {
            float lerp = SmoothStep(t / moveDuration);


            menuCamera.transform.position =
                Vector3.Lerp(camFrom, cameraStartPos, lerp);


            title.position =
                Vector3.Lerp(titleFrom, titleEndPos, lerp);


            buttonsPanel.anchoredPosition =
                Vector2.Lerp(buttonsStartPos, buttonsEndPos, lerp);


            buttonsCanvasGroup.alpha =
                Mathf.Lerp(0,1,lerp);


            creditsCanvasGroup.alpha =
                Mathf.Lerp(1,0,lerp);


            t += Time.deltaTime;
            yield return null;
        }


        creditsCanvasGroup.gameObject.SetActive(false);


        isShowingCredits = false;
        if (discordCanvasGroup != null)
            StartCoroutine(FadeDiscord(1f));
        isAnimating = false;
    }



    IEnumerator PlayIntro()
    {
        float t = 0;


        while(t < titleFadeDuration)
        {
            SetTitleAlpha(
                Mathf.Lerp(0,1,t/titleFadeDuration)
            );


            t += Time.deltaTime;
            yield return null;
        }


        SetTitleAlpha(1);


        yield return new WaitForSeconds(titleHoldTime);


        StartCoroutine(MoveButtons());


        t = 0;


        while(t < moveDuration)
        {
            float lerp = SmoothStep(t/moveDuration);


            title.position =
                Vector3.Lerp(titleStartPos,titleEndPos,lerp);


            t += Time.deltaTime;
            yield return null;
        }

        if (discordCanvasGroup != null)
            StartCoroutine(FadeDiscord(1f));
    }



    IEnumerator MoveButtons()
    {
        float t = 0;


        while(t < moveDuration)
        {
            float lerp = SmoothStep(t/moveDuration);


            buttonsPanel.anchoredPosition =
                Vector2.Lerp(buttonsStartPos,buttonsEndPos,lerp);


            buttonsCanvasGroup.alpha =
                Mathf.Lerp(0,1,lerp);


            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeDiscord(float targetAlpha)
    {
        if (discordCanvasGroup == null)
            yield break;

        discordCanvasGroup.interactable = targetAlpha > 0.99f;
        discordCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
        float startAlpha = discordCanvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, discordFadeDuration);

        while (elapsed < duration)
        {
            float lerp = SmoothStep(elapsed / duration);
            discordCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            elapsed += Time.deltaTime;
            yield return null;
        }

        discordCanvasGroup.alpha = targetAlpha;
    }



    void SetTitleAlpha(float alpha)
    {
        if(!titleRenderer) return;


        var c = titleRenderer.color;
        c.a = alpha;
        titleRenderer.color = c;
    }



    float SmoothStep(float t)
    {
        return t*t*(3-2*t);
    }
}
