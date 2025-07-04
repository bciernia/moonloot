using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string NpcName;

    private Waypoint _waypoint;
    private NPCMovement _npcMovement;
    private EnemyBrain _enemyBrain;
    
    private void Awake()
    {
        _waypoint = GetComponent<Waypoint>();
        _npcMovement = GetComponent<NPCMovement>();
        _enemyBrain = GetComponent<EnemyBrain>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = this;
            FindFirstObjectByType<InteractionManager>().SetInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.NPCSelected = null;
            FindFirstObjectByType<InteractionManager>().ClearInteractable();
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
        if (_waypoint) _waypoint.enabled = isEnabled;
        if (_npcMovement) _npcMovement.enabled = isEnabled;
    }

    private void DisableNpcMovement() => SetNpcMovementEnabled(false);
    public void EnableNpcMovement() => SetNpcMovementEnabled(true);

    public string GetInteractionText() => $"Talk to: {NpcName}";
}
