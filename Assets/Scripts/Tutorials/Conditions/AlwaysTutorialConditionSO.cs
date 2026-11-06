using UnityEngine;

[CreateAssetMenu(fileName = "AlwaysTutorialCondition", menuName = "Tutorials/Conditions/Always")]
public sealed class AlwaysTutorialConditionSO : TutorialConditionSO
{
    public override bool IsMet(Player player) => player != null;
}
