using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorUI : MonoBehaviour
{
    private RectTransform _cursorTransform;
    private Canvas _priorityCanvas;
    private Graphic _cursorGraphic;
    private bool _createdPriorityCanvas;
    private bool _originalCanvasEnabled;
    private RenderMode _originalRenderMode;
    private bool _originalOverrideSorting;
    private int _originalSortingOrder;
    private bool _originalRaycastTarget;

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

    public void SetAboveModal(bool enabled)
    {
        if (enabled)
        {
            if (_priorityCanvas == null)
            {
                _priorityCanvas = GetComponent<Canvas>();
                _createdPriorityCanvas = _priorityCanvas == null;
                if (_createdPriorityCanvas)
                    _priorityCanvas = gameObject.AddComponent<Canvas>();

                _originalRenderMode = _priorityCanvas.renderMode;
                _originalOverrideSorting = _priorityCanvas.overrideSorting;
                _originalSortingOrder = _priorityCanvas.sortingOrder;
                _originalCanvasEnabled = _priorityCanvas.enabled;
            }

            // Keep a created Canvas for reuse. Destroy is deferred until the end
            // of the frame, so a modal opened immediately afterwards could pick
            // up a Canvas that is already scheduled for destruction.
            _priorityCanvas.enabled = true;
            _priorityCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _priorityCanvas.overrideSorting = true;
            _priorityCanvas.sortingOrder = 32767;

            if (_cursorGraphic == null)
            {
                _cursorGraphic = GetComponent<Graphic>();
                if (_cursorGraphic == null)
                    _cursorGraphic = GetComponentInChildren<Graphic>(true);
                if (_cursorGraphic != null)
                    _originalRaycastTarget = _cursorGraphic.raycastTarget;
            }

            if (_cursorGraphic != null)
                _cursorGraphic.raycastTarget = false;

            return;
        }

        if (_cursorGraphic != null)
            _cursorGraphic.raycastTarget = _originalRaycastTarget;

        if (_priorityCanvas != null)
        {
            if (_createdPriorityCanvas)
            {
                _priorityCanvas.enabled = false;
            }
            else
            {
                _priorityCanvas.renderMode = _originalRenderMode;
                _priorityCanvas.overrideSorting = _originalOverrideSorting;
                _priorityCanvas.sortingOrder = _originalSortingOrder;
                _priorityCanvas.enabled = _originalCanvasEnabled;
            }
        }

    }
}
