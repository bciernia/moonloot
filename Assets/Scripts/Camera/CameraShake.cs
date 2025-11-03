using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmount = 0.2f;
    private bool isShaking = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShakeIt();
        }
    }

    private IEnumerator Shake()
    {
        if (isShaking) yield return null;

        isShaking = true;
        var originalPos = transform.localPosition;

        var elapsedTime = 0.0f;

        while (elapsedTime < shakeDuration)
        {
            var x = Random.Range(-1f, 1f) * shakeAmount;
            var y = Random.Range(-1f, 1f) * shakeAmount;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }

    public void ShakeIt()
    {
        StartCoroutine(Shake());
    }
}
