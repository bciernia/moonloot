using System;
using UnityEngine;

public class OpenCloseDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite ClosedDoorSprite;
    [SerializeField] private Sprite OpenedDoorSprite;
    [SerializeField] private string ConnectedRoom;
    
    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _collider2D;
    
    private bool AreOpened { get; set; }
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        AreOpened = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().SetInteractable(this);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<InteractionManager>().ClearInteractable();
        }
    }


    public void Interact()
    {
        AreOpened = !AreOpened;

        if (AreOpened)
        {
            _spriteRenderer.sprite = OpenedDoorSprite;
            _collider2D.enabled = false;
        }
        else
        {
            _spriteRenderer.sprite = ClosedDoorSprite;
            _collider2D.enabled = true;
        }
    }

    public string GetInteractionText() => $"Open: {ConnectedRoom}";
}
