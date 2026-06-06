using System;
using EasyTalk.Controller;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : Singleton<DialogueManager>
{
    public NPCInteraction NPCSelected { get; set; }
    [SerializeField] public GameObject PlayerUI;
    [SerializeField] public GameObject DialogueUI;

    private bool _dialogueStarted;
    private PlayerInput _playerInput;
    private GameObject _player;

    public static bool DialogueInProgress => Instance != null && Instance._dialogueStarted;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;
    
    protected override void Awake()
    {
        base.Awake();
        _playerInput = FindAnyObjectByType<PlayerInput>();
        _player = GameObject.FindWithTag("Player");
        
        _playerInput.actions["Main menu"].performed += _ => OnMainMenuPressed();
    }

    private void OnMainMenuPressed()
    {
        if (_dialogueStarted)
        {
            EndDialogue();
        }
    }

    public void StartDialogue()
    {
        if (_dialogueStarted || !NPCSelected) return;
        
        _dialogueStarted = true;
        OnDialogueStarted?.Invoke();
        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        
        if(!dialogueController.CurrentDialogue) return;
        
        SetCharacterInFrontOfNpc();
        PlayerUI.SetActive(false);
        DialogueUI.SetActive(true);

        var npcDialogueEntrySetter = NPCSelected.GetComponent<DialogueEntrySetter>();
        var entryId = "";
        if (npcDialogueEntrySetter)
        {
            entryId = npcDialogueEntrySetter.GetEntryId();
        }
        dialogueController.onStop.AddListener(EndDialogue);
        dialogueController.PlayDialogue(entryId);
    }

    public void ContinueDialogue()
    {
        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        if (dialogueController.CurrentDialogue)
        {
            dialogueController.Continue();
        }
    }

    private void SetCharacterInFrontOfNpc()
    {
        _player.GetComponent<PlayerMovement>().enabled = false;
        _player.GetComponent<PlayerAnimations>().SetIsMovingAnimation(false);
    }

    public void EndDialogue()
    {
        _player.GetComponent<PlayerMovement>().enabled = true;

        if (NPCSelected != null)
        {
            NPCSelected.EnableNpcMovement();
            var dialogueController = NPCSelected.GetComponent<DialogueController>();
            
            if (dialogueController.CurrentDialogue != null)
            {
                dialogueController.ExitDialogue();
            }
            
            dialogueController.onStop.RemoveListener(EndDialogue);
        }
        
        _dialogueStarted = false;
        OnDialogueEnded?.Invoke();
        PlayerUI.SetActive(true);
        DialogueUI.SetActive(false);

        // FindObjectOfType<FourthWallDialogueManager>()?.OnDialogueEnded();
    }

    public void SetWorldState(string state)
    {
        WorldStateManager.Instance.SetState(state);
    }
    
    public bool IsInDialogue() => _dialogueStarted;
}
