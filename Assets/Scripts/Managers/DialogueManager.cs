using System;
using EasyTalk.Controller;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    public NPCInteraction NPCSelected { get; set; }

    public bool IsInConversation { get; set; }
    private bool _dialogueStarted;
    private PlayerActions _playerActions;
    private GameObject _player;

    protected override void Awake()
    {
        base.Awake();
        _playerActions = new PlayerActions();
        _player = GameObject.FindWithTag("Player");
    }

    // public void Start()
    // {
    //     _playerActions.Dialogue.Interact.performed += ctx => StartDialogue();
    // }
    
    public void StartDialogue()
    {
        if (_dialogueStarted) return;

        if (!NPCSelected)
        {
            var playerDialogueController = _player.GetComponent<DialogueController>();
            playerDialogueController.PlayDialogue();
            return;
        }
        
        _dialogueStarted = true;
        // NPCSelected._interactionBox.SetActive(false);

        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        SetCharacterInFrontOfNpc();
    
        dialogueController.onStop.AddListener(EndDialogue);
        dialogueController.PlayDialogue();
    }
    
    private void SetCharacterInFrontOfNpc()
    {
        _player.GetComponent<PlayerMovement>().enabled = false;
        _player.GetComponent<PlayerAnimations>().SetIsMovingAnimation(false);
    }

    private void EndDialogue()
    {
        _player.GetComponent<PlayerMovement>().enabled = true;

        // if (NPCSelected != null)
        // {
             // NPCSelected._interactionBox.SetActive(true);
        // }

        NPCSelected.EnableNpcMovement();
        
        _dialogueStarted = false;

        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        
        dialogueController.onStop.RemoveListener(EndDialogue);
    }

    private void OnEnable()
    {
        _playerActions.Enable();
    }

    private void OnDisable()
    {
        _playerActions.Disable();
    }
}
