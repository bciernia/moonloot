using UnityEngine;

public class BlacksmithZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player.Instance.IsNearBlacksmith = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player.Instance.IsNearBlacksmith = false;
    }
}