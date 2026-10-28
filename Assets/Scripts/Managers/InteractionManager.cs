using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();
    private IInteractable _currentInteractable;

    private Action<InputAction.CallbackContext> _interactionAction;
    private Action<InputAction.CallbackContext> _interactionCanceledAction;
    private InputAction _interactionInputAction;
    
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _interactionBox;
    [SerializeField] private TextMeshProUGUI _interactionBoxTMP;

    private void Awake()
    {
        _interactionAction = ctx => Interact();
        _interactionCanceledAction = ctx => StopInteract();
    }

    private void OnEnable()
    {
        GameManager.OnGameModeChanged += OnGameModeChanged;
        // Covers managers enabled after the sceneLoaded callback; the shared
        // binder will reapply the context once the new scene is ready.
        if (Player.Instance != null)
            BindPlayerContext(Player.Instance);
    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnGameModeChanged;
        UnbindInput();
    }

    public void BindPlayerContext(Player player)
    {
        _playerTransform = player != null ? player.transform : null;
        BindPlayerAndInput(player);
        UpdateClosestInteractable();
    }

    private void BindPlayerAndInput(Player player)
    {
        var playerInput = player != null
            ? player.GetComponent<PlayerInput>()
            : FindAnyObjectByType<PlayerInput>();

        var interactionAction = playerInput != null && playerInput.actions != null
            ? playerInput.actions.FindAction("Interaction", false)
            : null;

        if (interactionAction == _interactionInputAction)
            return;

        UnbindInput();
        _playerInput = playerInput;
        _interactionInputAction = interactionAction;

        if (_interactionInputAction != null)
        {
            _interactionInputAction.performed += _interactionAction;
            _interactionInputAction.canceled += _interactionCanceledAction;
        }
        else if (_playerInput != null)
        {
            Debug.LogError("InteractionManager could not find the 'Interaction' input action.");
        }
    }

    private void UnbindInput()
    {
        if (_interactionInputAction != null)
        {
            _interactionInputAction.performed -= _interactionAction;
            _interactionInputAction.canceled -= _interactionCanceledAction;
        }

        _interactionInputAction = null;
        _playerInput = null;
    }
    
    private void Interact()
    {
        // if (GameManager.Instance.CurrentMode == GameMode.MainMenu || CombatManager.Instance.IsPlayerInCombat)
        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.MainMenu)
            return;

        _currentInteractable?.Interact();
    }
    
    private void StopInteract()
    {
        if (_currentInteractable == null)
            return;

        var condition =
            (_currentInteractable as MonoBehaviour)
            ?.GetComponent<IChestUnlockCondition>();

        condition?.StopInteract();
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
    
    private void OnGameModeChanged(GameMode mode)
    {
        if (mode == GameMode.MainMenu)
        {
            ClearAllInteractables();
        }
    }
    
    private void ClearAllInteractables()
    {
        _nearbyInteractables.Clear();
        ClearInteractable();
    }

    private void UpdateClosestInteractable()
    {
        RemoveNulls();

        if (_playerTransform == null && Player.Instance != null)
            _playerTransform = Player.Instance.transform;

        if (_playerTransform == null)
        {
            ClearInteractable();
            return;
        }

        // if (_nearbyInteractables.Count == 0 || CombatManager.Instance.IsPlayerInCombat)
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
            if (_interactionBox != null)
                _interactionBox.SetActive(true);
            if (_interactionBoxTMP != null)
                _interactionBoxTMP.text = interactable.GetInteractionText();
        }
        else
        {
            if (_interactionBox != null)
                _interactionBox.SetActive(false);
            if (_interactionBoxTMP != null)
                _interactionBoxTMP.text = "";
        }
    }

    private void ClearInteractable()
    {
        _currentInteractable = null;
        if (_interactionBox != null)
            _interactionBox.SetActive(false);
        if (_interactionBoxTMP != null)
            _interactionBoxTMP.text = "";
    }
    
    public void RefreshInteractable(IInteractable interactable)
    {
        UnregisterInteractable(interactable);
        RegisterInteractable(interactable);
    }
}
