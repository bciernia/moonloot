using UnityEngine;

public class BossRotation : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 30f;

    private void Update()
    {
        transform.Rotate(
            0f,
            0f,
            _rotationSpeed * Time.deltaTime
        );
    }
}