using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count == 0)
            {
                Debug.Log("Nothing UI-related was clicked");
                return;
            }

            Debug.Log("UI elements under cursor (top = first):");

            foreach (var result in results)
            {
                Debug.Log($"→ {result.gameObject.name}");
            }
        }
    }
}