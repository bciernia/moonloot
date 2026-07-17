using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatedEnvironmentElement : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private Vector2 firstDelayRange = new(2f, 6f);
    [SerializeField] private Vector2 repeatDelayRange = new(4f, 8f);

    [Header("Animation")]
    [SerializeField] private bool hideSpriteBetweenAnimations = true;
    [SerializeField] private string defaultTrigger = "Animate";
    [SerializeField] private string[] triggers;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private bool _animationFinished;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null && hideSpriteBetweenAnimations)
        {
            _spriteRenderer.enabled = false;
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(
            Random.Range(
                firstDelayRange.x,
                firstDelayRange.y));

        yield return AnimationLoop();
    }

    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            _animationFinished = false;

            if (_spriteRenderer != null && hideSpriteBetweenAnimations)
            {
                _spriteRenderer.enabled = true;
            }

            var trigger = GetRandomTrigger();

            _animator.ResetTrigger(trigger);
            _animator.SetTrigger(trigger);

            yield return new WaitUntil(() => _animationFinished);

            yield return new WaitForSeconds(
                Random.Range(
                    repeatDelayRange.x,
                    repeatDelayRange.y));
        }
    }

    private string GetRandomTrigger()
    {
        if (triggers == null || triggers.Length == 0)
        {
            return defaultTrigger;
        }

        return triggers[
            Random.Range(
                0,
                triggers.Length)];
    }

    /// <summary>
    /// Animation Event
    /// </summary>
    public void Hide()
    {
        if (_spriteRenderer != null && hideSpriteBetweenAnimations)
        {
            _spriteRenderer.enabled = false;
        }

        _animationFinished = true;
    }
}