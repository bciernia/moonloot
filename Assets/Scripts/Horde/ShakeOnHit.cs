using System.Collections;
using UnityEngine;

public class ShakeOnHit : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float duration = 0.25f;

    [SerializeField]
    private float strength = 0.28f;

    [SerializeField]
    private AnimationCurve falloff =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Vector3 _originalPosition;

    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (target == null)
            target = transform;

        _originalPosition = target.localPosition;
    }

    public void Shake()
    {
        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        target.localPosition = _originalPosition;

        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            var percent = elapsed / duration;

            var multiplier = falloff.Evaluate(percent);

            var offset = Random.insideUnitCircle * (strength * multiplier);

            target.localPosition =
                _originalPosition +
                new Vector3(
                    offset.x,
                    offset.y,
                    0f);

            yield return null;
        }

        target.localPosition = _originalPosition;
        _shakeCoroutine = null;
    }
}