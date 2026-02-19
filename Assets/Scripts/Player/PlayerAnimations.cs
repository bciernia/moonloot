using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int moveY = Animator.StringToHash("MoveY");
    private readonly int isMoving = Animator.StringToHash("IsMoving");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int revive = Animator.StringToHash("Revive");
    
    private Animator _animator;

    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
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

    public void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeAnimatorController)
    {
        _animator.runtimeAnimatorController = runtimeAnimatorController;
    }

    public void Step()
    {
        if (_audioSource == null) return;
        
        var currentFloorClip = SoundManager.Instance.GetCurrentFloorClip(transform.position);

        if (currentFloorClip != null)
        {
            _audioSource.PlayOneShot(currentFloorClip, 1f);
        }
    }
}
