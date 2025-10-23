using System;
using System.Linq;

public class DecisionPositiveDialogueFinish : FSMDecision
{
    private EnemyBrain _enemyBrain;
    private ActionStartDialogue _actionStartDialogue;
    private EnemyRelationship _enemyRelationship;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _actionStartDialogue = GetComponent<ActionStartDialogue>();
        _enemyRelationship = GetComponent<EnemyRelationship>();
    }

    public override bool Decide()
    {
        if (_actionStartDialogue.PositiveDialogueFinish)
        {
            var state = _enemyBrain.states.First(x => x.ID == "Chase");
            state.Transitions[1].TrueState = string.Empty;
            _enemyRelationship.SetIsCharacterFriendly(true);
            return true;
        }
        
        
        return false;
    }
}