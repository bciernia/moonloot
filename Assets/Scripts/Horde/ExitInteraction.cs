using UnityEngine;

public class ExitInteraction : MonoBehaviour, IInteractable
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
        }
    }
    
    public void Interact()
    {
        Debug.Log("EXIT USED");
        HordeManager.Instance.OnPlayerExit();
        Destroy(gameObject);
    }

    public string GetInteractionText() => "Leave map";

    public bool CanInteractInCombat() => true;
}