using EasyTalk.Controller;
using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string NpcName;

    private Waypoint _waypoint;
    private NPCMovement _npcMovement;
    private EnemyBrain _enemyBrain;
    private InteractionManager _interactionManager;

    private RescueNpc _rescueNpc;
    
    private void Awake()
    {
        _waypoint = GetComponent<Waypoint>();
        _npcMovement = GetComponent<NPCMovement>();
        _enemyBrain = GetComponent<EnemyBrain>();
        _rescueNpc = GetComponent<RescueNpc>();
        _interactionManager = FindFirstObjectByType<InteractionManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!DialogueManager.Instance.IsInDialogue())
            {
                DialogueManager.Instance.NPCSelected = this;
            }
            
            _interactionManager.RegisterInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (DialogueManager.Instance.NPCSelected == this)
            {
                DialogueManager.Instance.NPCSelected = null;
            }
            _interactionManager.UnregisterInteractable(this);
        }
    }

    public void Interact()
    {
        var enemyAnimator = GetComponent<EnemyAnimator>();
        var player = GameObject.FindGameObjectWithTag("Player");
        enemyAnimator.SetNpcPositionForDialogue(player.transform.position, gameObject.transform.position);
        
        if(_rescueNpc != null && !_rescueNpc.IsSaved())
        {
            _rescueNpc.SetSaveNpc();
            if (DialogueManager.Instance.NPCSelected == this)
            {
                DialogueManager.Instance.NPCSelected = null;
            }
            _interactionManager.UnregisterInteractable(this);
            enabled = false;
            
            return;
        }
        
        DialogueManager.Instance.StartDialogue();
        DisableNpcMovement();
    }

    private void SetNpcMovementEnabled(bool isEnabled)
    {
        if (_enemyBrain) _enemyBrain.enabled = isEnabled;
        if (_waypoint && _waypoint.HasAnyWaypoints) _waypoint.enabled = isEnabled;
        if (_npcMovement) _npcMovement.enabled = isEnabled;
    }

    private void DisableNpcMovement() => SetNpcMovementEnabled(false);
    public void EnableNpcMovement() => SetNpcMovementEnabled(true);

    public string GetInteractionText()
    {
        var npcName = GetComponent<NPCStatUpgrade>()?.GetNpcName();
        return string.IsNullOrEmpty(npcName) ? $"Talk to: {NpcName}" : $"Talk to: {npcName}";
    }
}
