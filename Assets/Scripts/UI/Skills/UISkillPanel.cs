using UnityEngine;

public class UISkillPanel : MonoBehaviour
{
    [SerializeField] private UISkillBtn[] skillButtons;

    private void OnEnable()
    {
        if (PlayerSkillManager.Instance != null)
            PlayerSkillManager.Instance.OnSkillsChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (PlayerSkillManager.Instance != null)
            PlayerSkillManager.Instance.OnSkillsChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        foreach (var btn in skillButtons)
            btn.RefreshState();
    }
}