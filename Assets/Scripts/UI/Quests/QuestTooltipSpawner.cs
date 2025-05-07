using UnityEngine;

public class QuestTooltipSpawner : TooltipSpawner
{
    public override bool CanCreateTooltip()
    {
        return true;
    }
    
    public override void UpdateTooltip(GameObject tooltip)
    {
        var status = GetComponent<QuestItemUI>().GetQuestStatus();
        tooltip.GetComponent<QuestTooltipUI>().Setup(status);
    }
}
