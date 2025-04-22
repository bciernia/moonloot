using UnityEngine;

public class QuestCompletion : MonoBehaviour
{
    [SerializeField] private Quest quest;
    [SerializeField] private string objective;

    public void CompleteObjective()
    {
        var questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.CompleteObjective(quest, objective);
    }
}
