using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private PlayerActions _playerActions;
    private IInteractable currentInteractable;

    private void Awake()
    {
        _playerActions = new PlayerActions();
        _playerActions.Dialogue.Enable();
        _playerActions.Dialogue.Interact.performed += ctx => Interact();
    }

    private void Interact()
    {
        currentInteractable?.Interact();
    }

    public void SetInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void ClearInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
        }
    }

    private void OnEnable() => _playerActions.Enable();
    private void OnDisable() => _playerActions.Disable();
}