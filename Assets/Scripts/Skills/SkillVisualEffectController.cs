using System.Collections;
using UnityEngine;

public class SkillVisualEffectController : MonoBehaviour
{
    [SerializeField] private Skill skill;

    private Animator _animator;
    public float Duration { get; set; }

    private static readonly int IsEnding = Animator.StringToHash("IsEnding");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(HandleAnimationSequence());
    }

    private IEnumerator HandleAnimationSequence()
    {
        yield return new WaitForSeconds(Duration);
        _animator.SetBool(IsEnding, true);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
