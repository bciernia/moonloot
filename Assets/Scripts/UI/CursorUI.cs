using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _hideAfterIdleSeconds = 2f;

    private RectTransform _cursorTransform;
    private Canvas _priorityCanvas;
    private Graphic _cursorGraphic;
    private bool _createdPriorityCanvas;
    private bool _originalCanvasEnabled;
    private RenderMode _originalRenderMode;
    private bool _originalOverrideSorting;
    private int _originalSortingOrder;
    private bool _originalRaycastTarget;
    private Vector2 _lastMousePosition;
    private float _idleTime;
    private bool _cursorShown = true;

    private void Awake()
    {
        _cursorTransform = GetComponent<RectTransform>();
        _cursorGraphic = GetComponent<Graphic>();
        if (_cursorGraphic == null)
            _cursorGraphic = GetComponentInChildren<Graphic>(true);
        if (_cursorGraphic != null)
            _originalRaycastTarget = _cursorGraphic.raycastTarget;

        _lastMousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        _idleTime = 0f;
        _cursorShown = true;
        if (_cursorGraphic != null)
            _cursorGraphic.enabled = true;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        var mousePosition = Mouse.current.position.ReadValue();
        _cursorTransform.position = mousePosition;

        if (mousePosition != _lastMousePosition)
        {
            _lastMousePosition = mousePosition;
            _idleTime = 0f;
            SetCursorShown(true);
            return;
        }

        if (IsMenuOrPaused())
        {
            _idleTime = 0f;
            SetCursorShown(true);
            return;
        }

        _idleTime += Time.unscaledDeltaTime;
        if (_idleTime >= _hideAfterIdleSeconds)
            SetCursorShown(false);
    }

    private bool IsMenuOrPaused()
    {
        if (LoadingSceneManager.Instance != null && LoadingSceneManager.Instance.IsInMainMenu())
            return true;

        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.MainMenu)
            return true;

        return PauseManager.Instance != null && PauseManager.Instance.IsGamePaused;
    }

    private void SetCursorShown(bool shown)
    {
        if (_cursorShown == shown)
            return;

        _cursorShown = shown;
        if (_cursorGraphic != null)
            _cursorGraphic.enabled = shown;
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
                // Preserve the Canvas for same-frame handoffs between modals,
                // but restore its normal rendering state so the cursor remains visible.
                _priorityCanvas.renderMode = _originalRenderMode;
                _priorityCanvas.overrideSorting = _originalOverrideSorting;
                _priorityCanvas.sortingOrder = _originalSortingOrder;
                _priorityCanvas.enabled = true;
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
