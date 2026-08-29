using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUI : MonoBehaviour
{
    private RectTransform _cursorTransform;

    private void Awake()
    {
        _cursorTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        _cursorTransform.position = Mouse.current.position.ReadValue();
    }
}