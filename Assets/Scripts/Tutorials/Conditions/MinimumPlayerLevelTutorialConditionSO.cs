using UnityEngine;

[CreateAssetMenu(fileName = "MinimumPlayerLevelCondition", menuName = "Tutorials/Conditions/Minimum Player Level")]
public sealed class MinimumPlayerLevelTutorialConditionSO : TutorialConditionSO
{
    [SerializeField, Min(1)] private int minimumLevel = 2;

    public override bool IsMet(Player player)
    {
        return player != null && player.PlayerStats != null && player.PlayerStats.Level >= minimumLevel;
    }
}
