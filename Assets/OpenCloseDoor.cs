using UnityEngine;

public class OpenCloseDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorsSO DoorsSO;
    
    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _collider2D;
    private DoorsSO _doors;
    
    private bool AreOpened { get; set; }
    private bool AreLocked { get; set; }
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        AreOpened = false;
        AreLocked = DoorsSO.KeyToOpen != null;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
        }
    }


    public void Interact()
    {
        if (AreLocked)
        {
            if (!InventoryController.Instance.UseItemById(DoorsSO.KeyToOpen.Id))
            {
                return;
            }

            AreLocked = false;
        }
        
        AreOpened = !AreOpened;

        if (AreOpened)
        {
            _spriteRenderer.sprite = DoorsSO.OpenedDoorSprite;
            _collider2D.enabled = false;
        }
        else
        {
            _spriteRenderer.sprite = DoorsSO.ClosedDoorSprite;
            _collider2D.enabled = true;
        }
        
        FindFirstObjectByType<InteractionManager>().RefreshInteractable(this);
    }

    public string GetInteractionText()
    {
        return AreLocked ? $"Need key: {DoorsSO.ConnectedRoom}" : AreOpened ? $"Close: {DoorsSO.ConnectedRoom}" : $"Open: {DoorsSO.ConnectedRoom}";
    }
}
