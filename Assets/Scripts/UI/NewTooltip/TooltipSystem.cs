using System;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;

    public Tooltip tooltip;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        current = this;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        tooltip.gameObject.SetActive(false);
    }

    public static void Show(string content, string header = "")
    {
        current.tooltip.SetText(content, header);
        current.tooltip.gameObject.SetActive(true);

        LeanTween.cancel(current.gameObject);
        LeanTween.alphaCanvas(current.canvasGroup, 1f, 0.3f).setEase(LeanTweenType.easeOutQuad);
    }

    public static void Hide()
    {
        LeanTween.cancel(current.gameObject);
        LeanTween.alphaCanvas(current.canvasGroup, 0f, 0.3f).setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => current.tooltip.gameObject.SetActive(false));
    }
}