using UnityEngine;

public class CameraBoundTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform targetFocusPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CameraFocusManager.Instance.SetFocus(targetFocusPoint);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CameraFocusManager.Instance.FocusPlayer();
    }
}