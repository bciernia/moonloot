using UnityEngine;
using UnityEngine.UI;

public class UISkillBtn : MonoBehaviour
{
    [SerializeField] private GameObject DescriptionPanel;
    [SerializeField] private Skill Skill;
    [SerializeField] private Image Image;

    private Button _button;
    
    private UISkillDescription _uiSkillDescription;

    private PlayerSkillProgress _skillProgress;
    
    private void Awake()
    {
        _uiSkillDescription = DescriptionPanel.GetComponent<UISkillDescription>();
        Image.sprite = Skill.Icon;
        _button = GetComponent<Button>();
        _skillProgress = FindAnyObjectByType<PlayerSkillProgress>();
        
        RefreshState();
    }

    public void SetDesc()
    {
        _uiSkillDescription.SetDescription(Skill);
    }

    public void RefreshState()
    {
        if (_skillProgress == null || Skill == null)
            return;

        var unlocked = _skillProgress.IsUnlocked(Skill);

        // if (_button != null)
        //     _button.interactable = unlocked;

        if (Image != null)
        {
            Image.color = unlocked
                ? Color.white
                : new Color(1f, 1f, 1f, 0.3f);
        }
    }
    
    public void Initialize(PlayerSkillProgress progress)
    {
        _skillProgress = progress;
    }
}
