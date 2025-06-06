using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCInfoManager : Singleton<NPCInfoManager>
{
    [SerializeField] private GameObject _npcInfoPanel;
    [SerializeField] private Image _healthBar;
    [SerializeField] private TextMeshProUGUI _npcName;
    
    private EnemyStatistics _enemyStatistics { get; set; }

    private void Update()
    {
        UpdateNpcUI();
    }
    
    public void ShowNpcInfo(EnemyStatistics enemyStatistics)
    {
        _enemyStatistics = enemyStatistics; 
        _npcInfoPanel.SetActive(true);
        _npcName.text = _enemyStatistics.Name;
    }

    public void HideNpcInfo()
    {
        _npcInfoPanel.SetActive(false);        
    }

    private void UpdateNpcUI()
    {
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _enemyStatistics.CurrentHP / _enemyStatistics.MaxHP,
            10f * Time.deltaTime);
    }
}