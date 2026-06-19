using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Level Up/Upgrade")]
public class LevelUpUpgradeSO : ScriptableObject
{
    public string UpgradeName;
    [TextArea]
    public string Description;

    public Sprite Icon;
    public StatBonus Bonus;
}