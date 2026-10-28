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
    private InputAction _mainMenuAction;
    private System.Action<InputAction.CallbackContext> _mainMenuCallback;

    public static bool DialogueInProgress => Instance != null && Instance._dialogueStarted;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;
    
    protected override void Awake()
    {
        base.Awake();
        _mainMenuCallback = _ => OnMainMenuPressed();
    }

    public void BindPlayerContext(Player player)
    {
        if (_mainMenuAction != null && _mainMenuCallback != null)
            _mainMenuAction.performed -= _mainMenuCallback;

        _player = player != null ? player.gameObject : null;
        _playerInput = player != null
            ? player.GetComponent<PlayerInput>()
            : FindAnyObjectByType<PlayerInput>();
        _mainMenuAction = _playerInput != null && _playerInput.actions != null
            ? _playerInput.actions.FindAction("Main menu", false)
            : null;

        if (_mainMenuAction != null && _mainMenuCallback != null)
            _mainMenuAction.performed += _mainMenuCallback;
    }

    private void OnDestroy()
    {
        if (_mainMenuAction != null && _mainMenuCallback != null)
            _mainMenuAction.performed -= _mainMenuCallback;
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
        if (_dialogueStarted || !NPCSelected || _player == null) return;
        
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
        if (_player != null && _player.TryGetComponent(out PlayerMovement playerMovement))
            playerMovement.enabled = true;

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
