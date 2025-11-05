using UnityEngine;

public class DialogueQuestHelper : MonoBehaviour
{
    private Quest quest;

    private void Awake()
    {
        quest = GetComponent<QuestGiver>().GetQuest();
    }

    //Used in dialogues
    public bool HasPlayerQuestItem(int itemIndex, int quantity) => InventoryController.Instance.HasUserQuestItem(quest.GetQuestItemName(itemIndex), quantity);

    //Used in dialogues
    public void TryRemoveQuestItems(int itemIndex, int quantity) => InventoryController.Instance.TryRemoveQuestItems(quest.GetQuestItemName(itemIndex), quantity);
}