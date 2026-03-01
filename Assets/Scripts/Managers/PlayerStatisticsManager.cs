using System.Globalization;
using TMPro;
using UnityEngine;

public class PlayerStatisticsManager : Singleton<PlayerStatisticsManager>
{
    [Header("Configuration")] 
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private TextMeshProUGUI Damage;
    [SerializeField] private TextMeshProUGUI PhysicalResistance;
    [SerializeField] private TextMeshProUGUI MagicResistance;
    [SerializeField] private TextMeshProUGUI DmgReduction;

    public void SetLevel(int level)
    {
        Level.text = level.ToString();
    }
    
    public void SetDamage(float dmg)
    {
        Damage.text = dmg.ToString(CultureInfo.InvariantCulture);
    }
    
    public void SetPhysicalResistance(float percent)
    {
        PhysicalResistance.text = percent.ToString("0") + "%";
    }

    public void SetMagicResistance(float percent)
    {
        MagicResistance.text = percent.ToString("0") + "%";
    }
    
    public void SetShieldReductionPercent(float percent)
    {
        DmgReduction.text = percent.ToString("0") + "%";
    }
}
