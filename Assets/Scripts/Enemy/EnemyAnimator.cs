using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private readonly int moveX = Animator.StringToHash("MovingX");
    private readonly int moveY = Animator.StringToHash("MovingY");
    private readonly int isMoving = Animator.StringToHash("IsMoving");
    private readonly int dead = Animator.StringToHash("Dead");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int damaged = Animator.StringToHash("Damaged");
    
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private Vector2 CurrentDirection { get; set; }
    
    private void Awake() {
        _animator = GetComponent<Animator>(); 
        _spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    public void SetIsMoving(bool value)
    {
        _animator.SetBool(isMoving, value);
    }

    public void SetMoveAnimation(Vector2 direction)
    {
        CurrentDirection = direction;
        _animator.SetFloat(moveX, direction.x);
        _animator.SetFloat(moveY, direction.y);
    }

    public void SetDeadAnimation()
    {
        _animator.SetTrigger(dead);
    }

    public void TryFlipSpriteX()
    {
        if (CurrentDirection.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
    }

    public void FlipSpriteXOff()
    {
        _spriteRenderer.flipX = false;
    }

    public void SetAttackAnimation()
    {
        _animator.SetTrigger(attack);
    }
    
    public void SetDamagedAnimation()
    {
        _animator.SetTrigger(damaged);
    }
}