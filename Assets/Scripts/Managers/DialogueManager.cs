using EasyTalk.Controller;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    public NPCInteraction NPCSelected { get; set; }

    private bool _dialogueStarted;
    private PlayerActions _playerActions;
    private GameObject _player;

    public static bool DialogueInProgress => Instance != null && Instance._dialogueStarted;

    protected override void Awake()
    {
        base.Awake();
        _playerActions = new PlayerActions();
        _player = GameObject.FindWithTag("Player");
    }

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

        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        SetCharacterInFrontOfNpc();

        var entryId = NPCSelected.GetComponent<DialogueEntrySetter>().GetEntryId();
        Debug.Log(entryId);
        dialogueController.onStop.AddListener(EndDialogue);
        dialogueController.PlayDialogue(entryId);
    }

    private void SetCharacterInFrontOfNpc()
    {
        _player.GetComponent<PlayerMovement>().enabled = false;
        _player.GetComponent<PlayerAnimations>().SetIsMovingAnimation(false);
    }

    private void EndDialogue()
    {
        _player.GetComponent<PlayerMovement>().enabled = true;

        if (NPCSelected != null)
        {
            NPCSelected.EnableNpcMovement();
        }

        _dialogueStarted = false;

        var dialogueController = NPCSelected.GetComponent<DialogueController>();
        dialogueController.onStop.RemoveListener(EndDialogue);

        FindObjectOfType<FourthWallDialogueManager>()?.OnDialogueEnded();
    }

    public bool IsDialogueRunning()
    {
        return _dialogueStarted;
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