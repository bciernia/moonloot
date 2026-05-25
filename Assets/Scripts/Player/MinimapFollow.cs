using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    private Transform _target;

    private void Start()
    {
        if (Player.Instance != null)
        {
            _target = Player.Instance.transform;
        }
    }

    private void LateUpdate()
    {
        if (!_target) return;

        transform.position = new Vector3(
            _target.position.x,
            _target.position.y,
            transform.position.z
        );
    }
}