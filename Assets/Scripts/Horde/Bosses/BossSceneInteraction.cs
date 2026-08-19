using UnityEngine;

public class BossSceneInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private NightLocationSO _bossLocation;

    
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
        if (_bossLocation == null)
        {
            Debug.LogWarning("Boss location is not assigned.");
            return;
        }

        ConfirmationManager.Instance.ShowConfirmation(
            "Do you want to fight with one of the moons?",
            confirmed =>
            {
                if (!confirmed)
                    return;
                FindFirstObjectByType<UIManager>().ShowStartBossNightPanel(_bossLocation);
            });
        
        FindFirstObjectByType<InteractionManager>()
            .UnregisterInteractable(this);
    }

    public string GetInteractionText() => "Fight with Udros";
}