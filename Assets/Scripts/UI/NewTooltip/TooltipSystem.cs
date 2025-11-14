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
    }

    public static void Hide()
    {
        current.tooltip.gameObject.SetActive(false);
    }
}