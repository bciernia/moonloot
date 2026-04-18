using EasyTalk.Controller;
using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string NpcName;

    private Waypoint _waypoint;
    private NPCMovement _npcMovement;
    private EnemyBrain _enemyBrain;
    private InteractionManager _interactionManager;

    private void Awake()
    {
        _waypoint = GetComponent<Waypoint>();
        _npcMovement = GetComponent<NPCMovement>();
        _enemyBrain = GetComponent<EnemyBrain>();
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
        DialogueManager.Instance.StartDialogue();

        var enemyAnimator = GetComponent<EnemyAnimator>();
        var player = GameObject.FindGameObjectWithTag("Player");
        
        DisableNpcMovement();
        enemyAnimator.SetNpcPositionForDialogue(player.transform.position, gameObject.transform.position);
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
