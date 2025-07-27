using UnityEngine;
using UnityEngine.UI;

public class SkillsSetter : MonoBehaviour
{
    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;
    
    public Skill Skill { get; set; }
    
    //TODO narazie dodajemy na sztywno jako Q i E skille
    public void SetQSkillBtn()
    {
        if (Skill != null)
        {
            QSkillImage.sprite = Skill.Icon;
            ChangeUsedSkill(0);
        }
    }
    
    public void SetESkillBtn()
    {
        if (Skill != null)
        {
            ESkillImage.sprite = Skill.Icon;
            ChangeUsedSkill(1);
        }
    }

    private void ChangeUsedSkill(int skillIndex)
    {
        SetSkillAsUnused(skillIndex);
        SetSkillInUse(skillIndex);
    }

    private void SetSkillInUse(int skillIndex)
    {
        SkillsManager.Instance.skills[skillIndex].skill = Skill;
        SkillsManager.Instance.skills[skillIndex].skill.IsInUse = true;
    }

    private void SetSkillAsUnused(int skillIndex)
    {
        SkillsManager.Instance.skills[skillIndex].skill.IsInUse = true;
    }
}
