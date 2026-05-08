using UnityEngine;

public class NightTrigger : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        var cycle = FindFirstObjectByType<DayNightCycle>();

        if (cycle != null)
        {
            cycle.ForceStartNight();
        }
        
        FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
    }

    public string GetInteractionText() => "Start night";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
            
        var manager = FindFirstObjectByType<InteractionManager>();
        if (manager != null)
        {
            manager.UnregisterInteractable(this);
        }
    }
}
