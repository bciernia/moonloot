using UnityEngine;
using UnityEngine.UI;

public class UISkillBtn : MonoBehaviour
{
    [SerializeField] private GameObject DescriptionPanel;
    [SerializeField] private Skill Skill;
    [SerializeField] private Image Image;

    private Button _button;
    private UISkillDescription _uiSkillDescription;

    private void Awake()
    {
        _uiSkillDescription = DescriptionPanel.GetComponent<UISkillDescription>();
        Image.sprite = Skill.Icon;
        _button = GetComponent<Button>();

        RefreshState();
    }

    public void SetDesc()
    {
        _uiSkillDescription.SetDescription(Skill);
    }

    public void RefreshState()
    {
        if (PlayerSkillManager.Instance == null || Skill == null)
            return;

        var unlocked = PlayerSkillManager.Instance.IsUnlocked(Skill);

        if (_button != null)
            _button.interactable = unlocked;

        if (Image != null)
        {
            Image.color = unlocked
                ? Color.white
                : new Color(1f, 1f, 1f, 0.3f);
        }
    }
}