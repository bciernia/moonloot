using Unity.Cinemachine;
using UnityEngine;

public class CameraFocusManager : Singleton<CameraFocusManager>
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private CinemachineCamera _camera;

    [SerializeField] private float _playerFollowSpeed = 35f;
    [SerializeField] private float _focusMoveSpeed = 6f;

    private bool _followPlayer;

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
        if (_cameraTarget == null || _currentTarget == null)
        {
            return;
        }
        var speed = _followPlayer
            ? _playerFollowSpeed
            : _focusMoveSpeed;

        _cameraTarget.position = Vector3.Lerp(
            _cameraTarget.position,
            _currentTarget.position,
            Time.deltaTime * speed);
        
        _camera.Lens.OrthographicSize = Mathf.Lerp(
            _camera.Lens.OrthographicSize,
            _targetOrthographicSize,
            Time.deltaTime * 5f);
    }

    public void SetFocus(Transform target, float size = 8.5f)
    {
        if (target == null)
        {
            return;
        }

        _currentTarget = target;
        _followPlayer = false;

        SetCameraOrthographicSize(size);
    }

    public void FocusPlayer()
    {
        if (_playerTransform == null)
        {
            return;
        }

        _currentTarget = _playerTransform;
        _followPlayer = true;

        SetCameraOrthographicSize();
    }

    public void SetCameraOrthographicSize(float size = 8.5f)
    {
        _targetOrthographicSize = size;
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