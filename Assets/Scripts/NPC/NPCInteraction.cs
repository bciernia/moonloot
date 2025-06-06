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

    private void DisableNpcMovement()
    {
        if (_enemyBrain)
        {
            _enemyBrain.enabled = false;
        }
        
        if (_waypoint)
        {
            _waypoint.enabled = false;
        }
        
        if (_npcMovement)
        {
            _npcMovement.enabled = false;
        }
    }

    public void EnableMovement()
    {
        if (_enemyBrain)
        {
            _enemyBrain.enabled = true;
        }
        
        if (_waypoint)
        {
            _waypoint.enabled = true;
        }
        
        if (_npcMovement)
        {
            _npcMovement.enabled = true;
        }
    }

    public string GetInteractionText() => $"Talk to: {NpcName}";
}
