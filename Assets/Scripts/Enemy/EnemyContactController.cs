using UnityEngine;

public class EnemyContactController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        collision.collider
            .GetComponent<PlayerMovement>()
            ?.EnterEnemyContact();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        collision.collider
            .GetComponent<PlayerMovement>()
            ?.ExitEnemyContact();
    }
}
