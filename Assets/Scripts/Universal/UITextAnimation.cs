using UnityEngine;

public class UITextAnimation : MonoBehaviour
{
    private Vector3 _baseScale;

    private void Start()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        var scale = 1f + Mathf.Sin(Time.unscaledTime * 3f) * 0.05f;

        transform.localScale = _baseScale * scale;
    }
}
