using System;
using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] public GameObject _interactionBox;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = this;
            FindFirstObjectByType<InteractionManager>().SetInteractable(this);
            _interactionBox.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = null;
            FindFirstObjectByType<InteractionManager>().ClearInteractable();
            _interactionBox.SetActive(false);
        }
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(); 
    }
}
