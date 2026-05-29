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
    public Vector3 creditsTitleOffset = new(0, .5f, 0);

    [Header("Camera")]
    public Camera menuCamera;

    [Header("Animation")]
    public float titleFadeDuration = 1f;
    public float titleHoldTime = 3f;
    public float moveDuration = 1.5f;

    public Vector3 titleEndOffset = new(0,3,0);
    public float buttonsMoveDistance = 500f;


    private Vector3 cameraStartPos;

    private Vector3 titleStartPos;
    private Vector3 titleEndPos;
    private Vector3 titleCreditsPos;


    private Vector2 buttonsStartPos;
    private Vector2 buttonsEndPos;


    private bool isShowingCredits;
    private bool isAnimating;


    [SerializeField]
    private TextMeshProUGUI version;



    void Start()
    {
        version.text = $"v{Application.version}";


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



    public void OpenCredits()
    {
        if(isAnimating) return;

        StartCoroutine(OpenCreditsRoutine());
    }



    IEnumerator OpenCreditsRoutine()
    {
        isAnimating = true;
        isShowingCredits = true;


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