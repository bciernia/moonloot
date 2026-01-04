using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private float speed;

    private PlayerInput _playerInput;
    private Rigidbody2D _rb2D;
    private Player _player;
    private Vector2 _moveDirection;
    private PlayerAnimations _playerAnimations;

    // [SerializeField] private float staminaTimer = 0f;
    // [SerializeField] private float staminaDecreaseTime = .125f;
    [SerializeField] private PlayerAim playerAim;

    private const float sprintSpeed = 2f;
    
    [SerializeField] private float worldMapSpeedMultiplier = 0.4f;
    private float _currentSpeedMultiplier = 1f;

    private PlayerStamina playerStamina;

    public bool limitToCameraView;
    private Camera mainCamera;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        _rb2D = GetComponent<Rigidbody2D>();
        _playerAnimations = GetComponent<PlayerAnimations>();
        playerStamina = GetComponent<PlayerStamina>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        ReadMovement();
        ReadAim();
    }

    private void FixedUpdate()
    {
        Move();
    }
    
    private void Move()
    {
        if (_player.PlayerStats.HP <= 0) return;

        var newPosition = CalculatedPosition();
        _rb2D.MovePosition(newPosition);
    }
    
    private Vector2 CalculatedPosition()
    {
        return _rb2D.position +
               _moveDirection * (speed * _currentSpeedMultiplier * Time.fixedDeltaTime);
    }


    public void ApplyDash(float dashAmount, float duration)
    {
        if (_moveDirection == Vector2.zero) return;
        StartCoroutine(DashCoroutine(dashAmount, duration));
    }

    private IEnumerator DashCoroutine(float dashAmount, float duration)
    {
        var originalSpeed = speed;
        speed += dashAmount;

        yield return new WaitForSeconds(duration);

        speed = originalSpeed;
    }
    
    // private IEnumerator DashCoroutine(float dashAmount, float duration)
    // {
    //     var originalSpeed = speed;
    //     speed += dashAmount;
    //
    //     yield return new WaitForSeconds(duration);
    //
    //     speed = originalSpeed;
    // }

    private void ReadMovement()
    {
        _moveDirection = _playerInput.actions["Move"].ReadValue<Vector2>().normalized;

        if (_moveDirection == Vector2.zero)
        {
            _playerAnimations.SetIsMovingAnimation(false);
            return;
        }
        
        _playerAnimations.SetIsMovingAnimation(true);
        _playerAnimations.SetMoveAnimation(_moveDirection);
    }
    
    private void ReadAim()
    {
        var aimDirection = _playerInput.actions["Aim"].ReadValue<Vector2>().normalized;
        if (playerAim != null)
            playerAim.UpdateAim(aimDirection);
    }

    private void OnGameModeChanged(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.WorldMap:
                _currentSpeedMultiplier = worldMapSpeedMultiplier;
                break;

            case GameMode.Location:
                _currentSpeedMultiplier = 1f;
                break;
        }
    }
    
    private void OnEnable()
    {
        GameManager.OnGameModeChanged += OnGameModeChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnGameModeChanged;
    }
}
