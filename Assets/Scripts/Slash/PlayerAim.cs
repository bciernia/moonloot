using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f;
    private Vector2 aimInput;
    private Vector2 lastAimDirection = Vector2.right;

    public void UpdateAim(Vector2 input)
    {
        if (input.sqrMagnitude > 0.01f)
            lastAimDirection = input.normalized;
    }

    private void Update()
    {
        float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}