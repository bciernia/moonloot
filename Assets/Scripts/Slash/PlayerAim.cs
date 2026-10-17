using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 5f;
    [SerializeField] private float _aimDistance = 0f;

    private float _outfitAimBonus;

    private Vector2 _lastAimDirection = Vector2.right;

    public float OutfitAimBonus => _outfitAimBonus;

    public void UpdateAim(Vector2 input)
    {
        if (input.sqrMagnitude > 0.01f)
        {
            _lastAimDirection = input.normalized;
        }
    }

    public void SetOutfitAimBonus(float bonus)
    {
        _outfitAimBonus = bonus;
    }

    private void Update()
    {
        var direction = _lastAimDirection;

        transform.localPosition =
            direction * _aimDistance;

        var angle = Mathf.Atan2(
            direction.y,
            direction.x) * Mathf.Rad2Deg;

        var targetRotation =
            Quaternion.AngleAxis(
                angle,
                Vector3.forward);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            _rotateSpeed * Time.deltaTime);
    }
}
