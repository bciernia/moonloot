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
        _portrait.sprite = data.Portrait;

        if (data is NPCStat stat)
        {
            _bonusText.text = stat.Bonus;
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

    public void OnClick(NPCData chosenNpc)
    {
        _uiManager.OnStartNightClicked(chosenNpc);
    }
}