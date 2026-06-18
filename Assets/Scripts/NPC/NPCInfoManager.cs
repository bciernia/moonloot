using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCInfoManager : Singleton<NPCInfoManager>
{
    [SerializeField] private GameObject _npcInfoPanel;
    [SerializeField] private Image _healthBar;
    [SerializeField] private TextMeshProUGUI _npcName;
    [SerializeField] private TextMeshProUGUI _enemyHpTMP;
    
    private EnemyStatistics _enemyStatistics { get; set; }
    private float _displayedHp;

    private void Update()
    {
        UpdateNpcUI();
    }
    
    public void ShowNpcInfo(EnemyStatistics enemyStatistics)
    {
        _enemyStatistics = enemyStatistics;
        _displayedHp = enemyStatistics.CurrentHP;
        _npcInfoPanel.SetActive(true);
        _npcName.text = _enemyStatistics.Name;
    }

    public void HideNpcInfo()
    {
        if (_npcInfoPanel.activeSelf)
        {
            _npcInfoPanel.SetActive(false);
            _enemyStatistics = null;
        }
    }

    private void UpdateNpcUI()
    {
        if (_enemyStatistics == null) return;
        
        _displayedHp = Mathf.MoveTowards(
            _displayedHp,
            _enemyStatistics.CurrentHP,
            100f * Time.deltaTime);
        
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _enemyStatistics.CurrentHP / _enemyStatistics.MaxHP,
            10f * Time.deltaTime);
        
        _enemyHpTMP.text =
            $"{FormatNumber(_displayedHp)}/{FormatNumber(_enemyStatistics.MaxHP)}";
    }

    private string FormatNumber(float value)
    {
        if (value >= 1_000_000_000)
            return $"{value / 1_000_000_000f:0.#}B";

        if (value >= 1_000_000)
            return $"{value / 1_000_000f:0.#}M";

        if (value >= 1_000)
            return $"{value / 1_000f:0.#}K";

        return Mathf.RoundToInt(value).ToString();
    }
}