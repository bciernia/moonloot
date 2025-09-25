using UnityEngine;

[RequireComponent(typeof(Quest))]
public class DialogueQuestHelper : MonoBehaviour
{
    private Quest quest;

    private void Awake()
    {
        quest = GetComponent<QuestGiver>().GetQuest();
    }

    //Used in dialogues
    public bool HasPlayerQuestItem() => InventoryController.Instance.HasUserQuestItem(quest.GetQuestItemName());

    //Used in dialogues
    public bool TryRemoveQuestItems(int quantity) => InventoryController.Instance.TryRemoveQuestItems(quest.GetQuestItemName(), quantity);
}