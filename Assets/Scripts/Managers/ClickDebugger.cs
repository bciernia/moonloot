using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDebugger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on: " + name);
    }
}