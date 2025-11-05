using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();
    private IInteractable _currentInteractable;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _interactionBox;
    [SerializeField] private TextMeshProUGUI _interactionBoxTMP;

    private void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
    }

    private void OnEnable()
    {
        _playerInput.actions["Interaction"].performed += ctx => Interact();
    }

    private void OnDisable()
    {
        _playerInput.actions["Interaction"].performed -= ctx => Interact();
    }
    
    private void Interact()
    {
        _currentInteractable?.Interact();
    }

    public void RegisterInteractable(IInteractable interactable)
    {
        if (!_nearbyInteractables.Contains(interactable))
            _nearbyInteractables.Add(interactable);

        UpdateClosestInteractable();
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        if (_nearbyInteractables.Contains(interactable))
            _nearbyInteractables.Remove(interactable);

        UpdateClosestInteractable();
    }

    private void UpdateClosestInteractable()
    {
        RemoveNulls();

        if (_nearbyInteractables.Count == 0)
        {
            ClearInteractable();
            return;
        }

        var minDistance = float.MaxValue;
        IInteractable closest = null;

        foreach (var i in _nearbyInteractables)
        {
            var mono = i as MonoBehaviour;
            if (mono == null) continue;

            var distance = Vector2.Distance(_playerTransform.position, mono.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = i;
            }
        }

        if (closest != _currentInteractable)
            SetInteractable(closest);
    }

    private void RemoveNulls()
    {
        _nearbyInteractables.RemoveAll(i => i == null || (i as MonoBehaviour) == null);
    }

    private void SetInteractable(IInteractable interactable)
    {
        _currentInteractable = interactable;

        if (interactable != null)
        {
            _interactionBox.SetActive(true);
            _interactionBoxTMP.text = interactable.GetInteractionText();
        }
        else
        {
            _interactionBox.SetActive(false);
            _interactionBoxTMP.text = "";
        }
    }

    private void ClearInteractable()
    {
        _currentInteractable = null;
        _interactionBox.SetActive(false);
        _interactionBoxTMP.text = "";
    }
    
    public void RefreshInteractable(IInteractable interactable)
    {
        UnregisterInteractable(interactable);
        RegisterInteractable(interactable);
    }
}