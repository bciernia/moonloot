using System.Collections.Generic;
using UnityEngine;

public class TaskTableInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private List<Quest> QuestList = new List<Quest>();
    [SerializeField] private GameObject QuestTable;
    
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
        QuestTableManager.Instance.PrepareQuestTable(QuestList);
        QuestTable.SetActive(true);
    }

    public string GetInteractionText() => "Check tasks";
}