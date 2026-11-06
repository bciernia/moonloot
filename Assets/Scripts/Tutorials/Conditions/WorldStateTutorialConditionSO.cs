using UnityEngine;

[CreateAssetMenu(fileName = "WorldStateTutorialCondition", menuName = "Tutorials/Conditions/World State")]
public sealed class WorldStateTutorialConditionSO : TutorialConditionSO
{
    [SerializeField] private string requiredState;
    [SerializeField] private bool requireStateToBeSet = true;

    public override bool IsMet(Player player)
    {
        if (player == null || string.IsNullOrWhiteSpace(requiredState) || WorldStateManager.Instance == null)
            return false;

        var isSet = WorldStateManager.Instance.HasState(requiredState);
        return requireStateToBeSet ? isSet : !isSet;
    }
}
