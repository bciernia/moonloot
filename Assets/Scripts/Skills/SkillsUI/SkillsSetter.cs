using System.Linq;
using UnityEngine;

public class SkillsSetter : MonoBehaviour
{
    public Skill Skill { get; set; }

    public void SetQSkillBtn()
    {
        if (Skill == null) return;
        ChangeUsedSkill(0);
    }

    public void SetESkillBtn()
    {
        if (Skill == null) return;
        ChangeUsedSkill(1);
    }

    private void ChangeUsedSkill(int skillIndex)
    {
        var skills = SkillsManager.Instance.skills;

        if (skillIndex < 0 || skillIndex >= skills.Count)
            return;

        var targetEntry = skills[skillIndex];

        var existingEntry = skills.FirstOrDefault(e => e.skill == Skill);

        if (existingEntry != null)
        {
            if (existingEntry == targetEntry)
                return;

            var temp = targetEntry.skill;
            targetEntry.skill = Skill;
            existingEntry.skill = temp;
        }
        else
        {
            targetEntry.skill = Skill;
        }

        SkillsManager.Instance.RefreshSlotUI();
    }
}