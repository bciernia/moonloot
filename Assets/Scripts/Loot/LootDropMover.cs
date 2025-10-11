using System.Collections;
using UnityEngine;

public class LootDropMover : MonoBehaviour
{
    public void MoveToPosition(Vector3 targetPosition, float duration = 0.3f)
    {
        StartCoroutine(MoveRoutine(targetPosition, duration));
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition, float duration)
    {
        var startPos = transform.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);

            var horizontal = Vector3.Lerp(startPos, targetPosition, t);
            var height = Mathf.Sin(t * Mathf.PI) * 0.3f;
            transform.position = horizontal + Vector3.up * height;
            yield return null;
        }

        transform.position = targetPosition;
    }
}