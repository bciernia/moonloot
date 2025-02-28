using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float speed;

    private PlayerActions _actions;
    private Rigidbody2D _rb2D;
    private Player _player;
    private Vector2 _moveDirection;
    private PlayerAnimations _playerAnimations;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _actions = new PlayerActions();
        _rb2D = GetComponent<Rigidbody2D>();
        _playerAnimations = GetComponent<PlayerAnimations>();
    }

    private void Update()
    {
        ReadMovement();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (_player.PlayerStats.HP <= 0) return;
        _rb2D.MovePosition(_rb2D.position + _moveDirection * (speed * Time.fixedDeltaTime));
    }

    private void ReadMovement()
    {
        _moveDirection = _actions.Movement.Move.ReadValue<Vector2>().normalized;

        if (_moveDirection == Vector2.zero)
        {
            _playerAnimations.SetIsMovingAnimation(false);
            return;
        }
        
        _playerAnimations.SetIsMovingAnimation(true);
        _playerAnimations.SetMoveAnimation(_moveDirection);
    }

    private void OnEnable()
    {
        _actions.Enable();   
    }

    private void OnDisable()
    {
        _actions.Disable();
    }
}
