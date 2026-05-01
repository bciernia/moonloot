using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISkillDescription : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private GameObject btnsPanel;
    [SerializeField] private GameObject lockedInfo;
    [SerializeField] private SkillsSetter skillsSetter;
    
    protected void Awake()
    {
        ResetDescription();
    }

    private void ResetDescription()
    {
        itemImage.gameObject.SetActive(false);
        title.text = "";
        description.text = "";
        btnsPanel.gameObject.SetActive(false);
        skillsSetter.Skill = null;
    }

    private void ShowButtonsIfSkillUnlocked(Skill chosenSkill)
    {
        var isUnlocked = PlayerSkillManager.Instance.IsUnlocked(chosenSkill);
        SetActionButtonsAndUnlockInfoSections(isUnlocked);
    }

    private void SetActionButtonsAndUnlockInfoSections(bool isUnlocked)
    {
        btnsPanel.SetActive(isUnlocked);
        lockedInfo.SetActive(!isUnlocked);
    }

    public void SetDescription(Skill skill)
    {
        ShowButtonsIfSkillUnlocked(skill);
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = skill.Icon;
        title.text = skill.Name;
        description.text = skill.Description;
        skillsSetter.Skill = skill;
    }
}
