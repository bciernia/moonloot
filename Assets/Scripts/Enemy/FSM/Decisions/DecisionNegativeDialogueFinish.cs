using System;
using System.Linq;

public class DecisionNegativeDialogueFinish : FSMDecision
{
    private EnemyBrain _enemyBrain;
    private EnemyRelationship _enemyRelationship;
    private ActionStartDialogue _actionStartDialogue;
    private NPCInteraction _npcInteraction;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyRelationship = GetComponent<EnemyRelationship>();
        _actionStartDialogue = GetComponent<ActionStartDialogue>();
        _npcInteraction = GetComponent<NPCInteraction>();
    }

    public override bool Decide()
    {
        if (_actionStartDialogue.NegativeDialogueFinish)
        {
            var state = _enemyBrain.states.First(x => x.ID == "Chase");
            state.Transitions[1].TrueState = "Attack";
            _npcInteraction.enabled = false;
            _enemyRelationship.SetCharacterAsEnemy();
            return true;
        }

        return false;
    }
    
    
}