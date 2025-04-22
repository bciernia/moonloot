using System;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] public GameObject _interactionBox;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = this;
            _interactionBox.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = null;
            _interactionBox.SetActive(false);
        }
    }
}
