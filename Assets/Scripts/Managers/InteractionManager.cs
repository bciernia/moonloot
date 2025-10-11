using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private PlayerActions _playerActions;
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();
    private IInteractable _currentInteractable;
    
    [SerializeField] private Transform _playerTransform;
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
    
    public void RefreshInteractable(IInteractable interactable)
    {
        ClearInteractable();
        SetInteractable(interactable);
    }
    
    private void OnEnable() => _playerActions.Enable();
    private void OnDisable() => _playerActions.Disable();
}