using System;
using UnityEngine;
using UnityEngine.UI;

public class UISkillBtn : MonoBehaviour
{
    [SerializeField] private GameObject DescriptionPanel;
    [SerializeField] private Skill Skill;
    [SerializeField] private Image Image;
    
    private UISkillDescription _uiSkillDescription;

    private void Awake()
    {
        _uiSkillDescription = DescriptionPanel.GetComponent<UISkillDescription>();
        Image.sprite = Skill.Icon;
    }

    public void SetDesc()
    {
        _uiSkillDescription.SetDescription(Skill);
    }
}
