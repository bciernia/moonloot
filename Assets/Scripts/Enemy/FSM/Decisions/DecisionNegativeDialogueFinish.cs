using System;
using System.Linq;

public class DecisionNegativeDialogueFinish : FSMDecision
{
    private EnemyBrain _enemyBrain;
    private ActionStartDialogue _actionStartDialogue;
    private NPCInteraction _npcInteraction;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
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
            _enemyBrain.SetEnemyLayer();
            return true;
        }

        return false;
    }
    
    
}