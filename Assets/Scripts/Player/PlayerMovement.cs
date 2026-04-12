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

    [SerializeField] private PlayerAim playerAim;

    [SerializeField] private float worldMapSpeedMultiplier = 0.4f;
    private float _currentSpeedMultiplier = 1f;

    public float BaseSpeed => speed;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        _rb2D = GetComponent<Rigidbody2D>();
        _playerAnimations = GetComponent<PlayerAnimations>();
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
        var bonusMultiplier = _player.PlayerStats.GetMoveSpeedMultiplier();
        
        return _rb2D.position +
               _moveDirection * (speed * _currentSpeedMultiplier * bonusMultiplier *Time.fixedDeltaTime);
    }

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        StartCoroutine(SpeedMultiplierCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedMultiplierCoroutine(float multiplier, float duration)
    {
        _currentSpeedMultiplier *= multiplier;

        yield return new WaitForSeconds(duration);

        _currentSpeedMultiplier /= multiplier;
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
        
        CombatStatsManager.Instance.UpdateDistance(transform);
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

    public float GetFinalSpeed()
    {
        var baseMultiplier = _player.PlayerStats.GetMoveSpeedMultiplier();
        var currentMultiplier = _currentSpeedMultiplier <= 0 ? 1f : _currentSpeedMultiplier;

        return speed * currentMultiplier * baseMultiplier;
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        GameManager.OnGameModeChanged += OnGameModeChanged;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnGameModeChanged;
    }
}
