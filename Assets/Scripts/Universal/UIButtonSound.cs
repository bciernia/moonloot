using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    public void OnPointerEnter(
        PointerEventData eventData)
    {
        Debug.Log("A");
        SoundManager.Instance.PlaySound(
            SoundType.UI_HOVER);
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        SoundManager.Instance.PlaySound(
            SoundType.UI_CLICK);
    }
}