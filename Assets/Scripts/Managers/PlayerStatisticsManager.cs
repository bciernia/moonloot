using System.Globalization;
using TMPro;
using UnityEngine;

public class PlayerStatisticsManager : Singleton<PlayerStatisticsManager>
{
    [Header("Configuration")] 
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private TextMeshProUGUI Damage;
    [SerializeField] private TextMeshProUGUI DmgResistance;
    [SerializeField] private TextMeshProUGUI MagicResistance;

    public void SetLevel(int level)
    {
        Level.text = level.ToString();
    }
    
    public void SetDamage(float dmg)
    {
        Damage.text = dmg.ToString(CultureInfo.InvariantCulture);
    }
    
    public void SetDmgResistance(float dmgResistance)
    {
        DmgResistance.text = dmgResistance.ToString(CultureInfo.InvariantCulture);
    }    
    
    public void SetMagicResistance(float magicResistance)
    {
        MagicResistance.text = magicResistance.ToString(CultureInfo.InvariantCulture);
    }    
}
