using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string NpcName; 
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = this;
            FindFirstObjectByType<InteractionManager>().SetInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = null;
            FindFirstObjectByType<InteractionManager>().ClearInteractable();
        }
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(); 
    }

    public string GetInteractionText() => $"Talk to: {NpcName}";
}
