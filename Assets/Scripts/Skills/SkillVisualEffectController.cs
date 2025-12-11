using System.Collections;
using UnityEngine;

public class SkillVisualEffectController : MonoBehaviour
{
    private Animator _animator;
    private Coroutine animationRoutine;

    public float Duration { get; set; }

    private static readonly int IsEnding = Animator.StringToHash("IsEnding");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void RestartAnimation()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimationSequence());
    }

    private IEnumerator AnimationSequence()
    {
        yield return new WaitForSeconds(Duration); //Bo zostawiamy jescze 1 sekunde na zakończenie animacji.
        _animator.SetBool(IsEnding, true);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}