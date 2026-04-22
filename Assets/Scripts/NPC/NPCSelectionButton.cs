using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCSelectionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _professionText;
    [SerializeField] private TextMeshProUGUI _bonusText;
    [SerializeField] private Image _portrait;

    private UIManager _uiManager;

    public void Setup(NPCData data, UIManager uiManager)
    {
        _uiManager = uiManager;
        
        _nameText.text = data.Name;
        _professionText.text = data.Profession;
        _portrait.sprite = data.Character.GetComponent<SpriteRenderer>().sprite;

        if (data is NPCStat)
        {
            _bonusText.text = FormatBonuses(data);
        }
        else if (data is NPCHero)
        {
            _bonusText.text = "He will help from guild";
        }
        else if (data is NPCMerchant)
        {
            _bonusText.text = "Buy some things";
        }
    }

    public void OnClick(VillageNpcData chosenNpc)
    {
        _uiManager.OnStartNightClicked(chosenNpc);
    }
    
    private string FormatBonuses(NPCData data)
    {
        if (data.UpgradeLevels[0].Bonuses == null || data.UpgradeLevels[0].Bonuses.Count == 0)
            return "";

        System.Text.StringBuilder sb = new();

        foreach (var bonus in data.UpgradeLevels[0].Bonuses)
        {
            var statName = GetReadableName(bonus.Type);
            var getBonusValue = GetBonusValue(bonus);
            
            sb.AppendLine($"+{getBonusValue} {statName}");
        }

        return sb.ToString();
    }

    private string GetBonusValue(StatBonus bonus)
    {
        if (bonus.Type == BonusType.Damage ||
            bonus.Type == BonusType.MoveSpeed ||
            bonus.Type == BonusType.Crit)
        {
            return bonus.Value + "%";
        }

        return bonus.Value.ToString(CultureInfo.InvariantCulture);
    }

    private string GetReadableName(BonusType type)
    {
        return type switch
        {
            BonusType.Damage => "Damage",
            BonusType.MoveSpeed => "Move Speed",
            BonusType.MaxHp => "Max HP",
            BonusType.Crit => "Crit Chance",
            _ => type.ToString()
        };
    }
}