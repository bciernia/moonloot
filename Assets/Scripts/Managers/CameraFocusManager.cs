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

        _currentTarget = _playerTransform;
        _targetOrthographicSize = _camera.Lens.OrthographicSize;
        _followPlayer = true;
        
        if (_cameraTarget != null && _playerTransform != null)
        {
            _cameraTarget.position = _playerTransform.position;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_zoomSequenceKey) && _zoomSequenceCoroutine == null)
            _zoomSequenceCoroutine = StartCoroutine(PlayZoomSequence());

        if (Input.GetKeyDown(_cameraFollowToggleKey))
            ToggleCameraFollow();

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
        
        if (_zoomSequenceCoroutine == null)
        {
            _camera.Lens.OrthographicSize = Mathf.Lerp(
                _camera.Lens.OrthographicSize,
                _targetOrthographicSize,
                Time.deltaTime * 5f);
        }
    }

    private IEnumerator PlayZoomSequence()
    {
        yield return AnimateCameraSize(_zoomSequenceStartSize);
        yield return new WaitForSeconds(_zoomSequencePause);

        yield return AnimateCameraSize(_zoomSequenceWideSize);
        yield return new WaitForSeconds(_zoomSequencePause);

        yield return AnimateCameraSize(_zoomSequenceReturnSize);
        _targetOrthographicSize = _zoomSequenceReturnSize;
        _zoomSequenceCoroutine = null;
    }

    private IEnumerator AnimateCameraSize(float targetSize)
    {
        var startSize = _camera.Lens.OrthographicSize;
        var duration = Mathf.Clamp(_zoomSequenceTransitionDuration, 0.01f, 2f);
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
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

    public void SetCameraOrthographicSize(float size = 8.5f)
    {
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
