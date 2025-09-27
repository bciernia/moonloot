using System;

public class ActionStartDialogue : FSMAction
{
    public bool PositiveDialogueFinish { get; set; } = false;
    public bool NegativeDialogueFinish { get; set; } = false;

    private EnemyAnimator _enemyAnimator;
    private EnemyRelationship _enemyRelationship;

    private void Awake()
    {
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemyRelationship = GetComponent<EnemyRelationship>();
    }

    public override void Act()
    {
        if (PositiveDialogueFinish || NegativeDialogueFinish) return;
        
        _enemyAnimator.SetIsMoving(false);
        DialogueManager.Instance.StartDialogue();        
    }

    public void SetPositiveDialogueFinish()
    {
        PositiveDialogueFinish = true;
    }

    public void SetNegativeDialogueFinish()
    {
        NegativeDialogueFinish = true;
    }
}
