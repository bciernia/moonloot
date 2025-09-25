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
    public bool HasPlayerQuestItem(int quantity) => InventoryController.Instance.HasUserQuestItem(quest.GetQuestItemName(), quantity);

    //Used in dialogues
    public void TryRemoveQuestItems(int quantity) => InventoryController.Instance.TryRemoveQuestItems(quest.GetQuestItemName(), quantity);
}