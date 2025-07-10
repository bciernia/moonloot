using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [CanBeNull] private static LTDescr delay;
    public string header;
    
    [Multiline]
    public string content;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        delay = LeanTween.delayedCall(.5f, () =>
        {
            TooltipSystem.Show(content, header);
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (delay != null) LeanTween.cancel(delay.uniqueId);
        TooltipSystem.Hide();
    }
}