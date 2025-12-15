using System;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
#pragma warning disable UDR0001
    private static TooltipSystem current;
#pragma warning restore UDR0001

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