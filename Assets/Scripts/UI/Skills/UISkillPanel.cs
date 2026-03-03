using UnityEngine;

public class UISkillPanel : MonoBehaviour
{
    [SerializeField] private UISkillBtn[] skillButtons;

    private PlayerSkillProgress _progress;

    private void Awake()
    {
        _progress = FindAnyObjectByType<PlayerSkillProgress>();
        
        foreach (var btn in skillButtons)
            btn.Initialize(_progress);
    }

    private void OnEnable()
    {
        if (_progress != null)
            _progress.OnSkillsChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (_progress != null)
            _progress.OnSkillsChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        foreach (var btn in skillButtons)
            btn.RefreshState();
    }
}