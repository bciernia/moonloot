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

    public void SetDescription(Skill skill)
    {
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = skill.Icon;
        title.text = skill.Name;
        description.text = skill.Description;
        btnsPanel.gameObject.SetActive(true);
        skillsSetter.Skill = skill;
    }
}
