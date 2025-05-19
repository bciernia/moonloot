using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private PlayerActions _playerActions;
    private IInteractable _currentInteractable;
    [SerializeField] private GameObject _interactionBox;
    [SerializeField] private TextMeshProUGUI _interactionBoxTMP;

    private void Awake()
    {
        _playerActions = new PlayerActions();
        _playerActions.Dialogue.Enable();
        _playerActions.Dialogue.Interact.performed += ctx => Interact();
    }

    private void Interact()
    {
        _currentInteractable?.Interact();
    }

    public void SetInteractable(IInteractable interactable)
    {
        _currentInteractable = interactable;
        _interactionBox.SetActive(true);
        _interactionBoxTMP.text = interactable.GetInteractionText();
    }

    public void ClearInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
            _interactionBox.SetActive(false);
            _interactionBoxTMP.text = null;
        }
    }
    
    private void OnEnable() => _playerActions.Enable();
    private void OnDisable() => _playerActions.Disable();
}