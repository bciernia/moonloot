using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFocusManager : Singleton<CameraFocusManager>
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private CinemachineCamera _camera;

    [SerializeField] private float _playerFollowSpeed = 35f;
    [SerializeField] private float _focusMoveSpeed = 6f;

    [Header("Camera zoom test sequence")]
    [SerializeField] private KeyCode _zoomSequenceKey = KeyCode.F8;
    [SerializeField, Min(0f)] private float _zoomSequenceStartSize = 3.5f;
    [SerializeField, Min(0f)] private float _zoomSequenceWideSize = 25f;
    [SerializeField, Min(0f)] private float _zoomSequenceReturnSize = 8.5f;
    [SerializeField, Min(0f)] private float _zoomSequencePause = 3f;
    [SerializeField, Range(0.01f, 2f)] private float _zoomSequenceTransitionDuration = 1.8f;

    [Header("Camera follow toggle")]
    [SerializeField] private KeyCode _cameraFollowToggleKey = KeyCode.F9;

    private bool _followPlayer;
    private bool _cameraFollowLocked;
    private Coroutine _zoomSequenceCoroutine;

    private Transform _currentTarget;
    private float _targetOrthographicSize;
    
    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        _followPlayer = true;
        BindPlayerContext(Player.Instance);
    }

    public void BindPlayerContext(Player player)
    {
        _playerTransform = player != null ? player.transform : null;

        if (_cameraTarget == null)
        {
            var targetObject = GameObject.Find("CameraTarget");
            _cameraTarget = targetObject != null
                ? targetObject.transform
                : _playerTransform;
        }

        if (_camera == null && player != null)
            _camera = player.GetComponentInChildren<CinemachineCamera>(true);

        if (_camera == null)
            _camera = FindFirstObjectByType<CinemachineCamera>();

        if (_camera != null && _cameraTarget != null)
            _camera.Target.TrackingTarget = _cameraTarget;

        _currentTarget = _playerTransform;
        _followPlayer = true;

        if (_camera != null)
            _targetOrthographicSize = _camera.Lens.OrthographicSize;

        if (_cameraTarget != null && _playerTransform != null)
            _cameraTarget.position = _playerTransform.position;
    }

    private void Update()
    {
        // Debug shortcuts kept for later, but disabled during normal gameplay.
        // if (Input.GetKeyDown(_zoomSequenceKey) && _zoomSequenceCoroutine == null)
        //     _zoomSequenceCoroutine = StartCoroutine(PlayZoomSequence());

        // if (Input.GetKeyDown(_cameraFollowToggleKey))
        //     ToggleCameraFollow();

        if (_cameraTarget == null || _currentTarget == null)
        {
            return;
        }
        var speed = _followPlayer
            ? _playerFollowSpeed
            : _focusMoveSpeed;

        if (!_cameraFollowLocked)
        {
            _cameraTarget.position = Vector3.Lerp(
                _cameraTarget.position,
                _currentTarget.position,
                Time.deltaTime * speed);
        }
        
        if (_camera != null && _zoomSequenceCoroutine == null)
        {
            _camera.Lens.OrthographicSize = Mathf.Lerp(
                _camera.Lens.OrthographicSize,
                _targetOrthographicSize,
                Time.deltaTime * 5f);
        }
    }

    private IEnumerator PlayZoomSequence()
    {
        if (_camera == null)
        {
            _zoomSequenceCoroutine = null;
            yield break;
        }

        yield return AnimateCameraSize(_zoomSequenceStartSize);
        yield return new WaitForSeconds(_zoomSequencePause);

        yield return AnimateCameraSize(_zoomSequenceWideSize);
        yield return new WaitForSeconds(_zoomSequencePause);

        yield return AnimateCameraSize(_zoomSequenceReturnSize);
        _targetOrthographicSize = _zoomSequenceReturnSize;
        _zoomSequenceCoroutine = null;
    }

    public float CurrentOrthographicSize => _camera != null
        ? _camera.Lens.OrthographicSize
        : _targetOrthographicSize > 0f ? _targetOrthographicSize : 8.5f;

    public IEnumerator AnimateCameraSizeUnscaled(float targetSize, float duration)
    {
        CancelZoomSequence();
        if (_camera == null)
            yield break;

        _zoomSequenceCoroutine = StartCoroutine(AnimateCameraSize(targetSize, duration, true));
        yield return _zoomSequenceCoroutine;
        _targetOrthographicSize = targetSize;
        _zoomSequenceCoroutine = null;
    }

    private IEnumerator AnimateCameraSize(float targetSize, float duration = -1f, bool useUnscaledTime = false)
    {
        var startSize = _camera.Lens.OrthographicSize;
        duration = duration > 0f
            ? Mathf.Clamp(duration, 0.01f, 2f)
            : Mathf.Clamp(_zoomSequenceTransitionDuration, 0.01f, 2f);
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            // SmoothStep gives a gentle start and stop while keeping the transition within duration.
            progress = progress * progress * (3f - 2f * progress);
            _camera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        _camera.Lens.OrthographicSize = targetSize;
    }

    public void SetFocus(Transform target, float size = 8.5f)
    {
        if (target == null)
        {
            return;
        }

        _cameraFollowLocked = false;
        _currentTarget = target;
        _followPlayer = false;

        CancelZoomSequence();
        SetCameraOrthographicSize(size);
    }

    public void FocusPlayer()
    {
        if (_playerTransform == null)
        {
            return;
        }

        _cameraFollowLocked = false;
        _currentTarget = _playerTransform;
        _followPlayer = true;

        CancelZoomSequence();
        SetCameraOrthographicSize();
    }

    public void ToggleCameraFollow()
    {
        _cameraFollowLocked = !_cameraFollowLocked;

        if (_cameraFollowLocked)
            return;

        if (_playerTransform != null)
        {
            _currentTarget = _playerTransform;
            _followPlayer = true;
        }
    }

    public void LockCameraAtPlayerPosition(Transform playerTransform)
    {
        if (playerTransform == null)
            return;

        if (_playerTransform != playerTransform)
            BindPlayerContext(playerTransform.GetComponent<Player>());

        _currentTarget = playerTransform;
        _followPlayer = true;
        _cameraFollowLocked = true;

        if (_cameraTarget != null)
            _cameraTarget.position = playerTransform.position;
    }

    public void LockCameraAtPosition(Vector3 worldPosition)
    {
        _cameraFollowLocked = true;
        if (_cameraTarget != null)
            _cameraTarget.position = worldPosition;
    }

    public void UnlockCameraFollow()
    {
        _cameraFollowLocked = false;
        if (_playerTransform != null)
        {
            _currentTarget = _playerTransform;
            _followPlayer = true;
        }
    }

    public void SetCameraOrthographicSize(float size = 8.5f)
    {
        if (_camera == null)
            return;

        CancelZoomSequence();
        _targetOrthographicSize = size;
    }

    private void CancelZoomSequence()
    {
        if (_zoomSequenceCoroutine == null)
            return;

        StopCoroutine(_zoomSequenceCoroutine);
        _zoomSequenceCoroutine = null;
    }
    
    public bool IsAtTarget(float distance = 0.1f)
    {
        if (_cameraTarget == null || _currentTarget == null)
        {
            return false;
        }

        return Vector3.Distance(
            _cameraTarget.position,
            _currentTarget.position) <= distance;
    }
}
