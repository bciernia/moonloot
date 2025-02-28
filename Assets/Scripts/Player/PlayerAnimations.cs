using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");
    private readonly int isMoving = Animator.StringToHash("IsMoving");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int revive = Animator.StringToHash("Revive");
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    public void SetDeathAnimation()
    {
        _animator.SetTrigger(death);
    }    

    public void SetIsMovingAnimation(bool value)
    {
        _animator.SetBool(isMoving, value);
    }
    
    public void SetMoveAnimation(Vector2 dir)
    {
        _animator.SetFloat(moveX, dir.x);
        _animator.SetFloat(moveY, dir.y);    
    }
    
    public void ResetPlayer()
    {
        SetMoveAnimation(Vector2.down);
        _animator.SetTrigger(revive);
    }
}
