using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain), typeof(EnemyStatistics), typeof(Animator))]
public sealed class EnemyMimicDisguise : MonoBehaviour, IInteractable
{
    private const float InteractionRange = 1.25f;
    private const float FallbackDeathAnimationDuration = 1.52f;

    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private AnimationClip activationAnimation;

    private enum MimicState
    {
        Disguised,
        Activating,
        Active,
        Dying
    }

    private MimicState _state;

    private EnemyBrain _enemyBrain;
    private EnemyStatistics _enemyStatistics;
    private EnemySounds _enemySounds;
    private EnemySelector _enemySelector;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private SpriteRenderer _spriteRenderer;
    private InteractionManager _interactionManager;
    private Collider2D[] _colliders;
    private bool[] _colliderEnabledStates;
    private Coroutine _activationCoroutine;
    private bool _isRegisteredForInteraction;

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
        _enemySounds = GetComponent<EnemySounds>();
        _enemySelector = GetComponent<EnemySelector>();
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _enemyStatistics.OnDeath += HandleEnemyDeath;

        // SetChestAppearance();
        EnterDisguisedState();
    }

    private void Update()
    {
        if (_state != MimicState.Disguised)
            return;

        var player = Player.Instance;
        var isPlayerNearby = player != null &&
                             Vector2.Distance(transform.position, player.transform.position) <= InteractionRange;

        if (isPlayerNearby)
        {
            RegisterForInteraction();
        }
        else
        {
            UnregisterForInteraction();
        }
    }

    private void OnDisable()
    {
        UnregisterForInteraction();
    }

    private void OnDestroy()
    {
        if (_enemyStatistics != null)
            _enemyStatistics.OnDeath -= HandleEnemyDeath;

        UnregisterForInteraction();
    }

    public void Interact()
    {
        if (_state != MimicState.Disguised)
            return;

        if (PauseManager.Instance != null && PauseManager.Instance.pauseRequests > 0)
            return;

        if (_activationCoroutine == null)
            _activationCoroutine = StartCoroutine(ActivateAfterAnimation());
    }

    public string GetInteractionText() => "Open chest";

    private void EnterDisguisedState()
    {
        _state = MimicState.Disguised;
        _enemySounds?.SetIdleSoundsEnabled(false);

        _enemyBrain.enabled = false;

        if (_navMeshAgent != null)
            _navMeshAgent.enabled = false;

        _animator.enabled = false;

        if (_enemySelector != null)
        {
            _enemySelector.NoSelectionCallback();
            _enemySelector.enabled = false;
        }

        _colliders = GetComponentsInChildren<Collider2D>(true);
        _colliderEnabledStates = new bool[_colliders.Length];

        for (var i = 0; i < _colliders.Length; i++)
        {
            _colliderEnabledStates[i] = _colliders[i].enabled;
            _colliders[i].enabled = false;
        }
    }

    private void SetChestAppearance()
    {
        if (_spriteRenderer == null || chestPrefab == null)
            return;

        var chestRenderer = chestPrefab.GetComponentInChildren<SpriteRenderer>(true);
        if (chestRenderer == null)
        {
            Debug.LogWarning($"{name}: the assigned chest prefab has no SpriteRenderer.", this);
            return;
        }

        _spriteRenderer.sprite = chestRenderer.sprite;
        _spriteRenderer.color = chestRenderer.color;
    }

    private IEnumerator ActivateAfterAnimation()
    {
        _state = MimicState.Activating;
        UnregisterForInteraction();

        if (activationAnimation != null)
        {
            var duration = activationAnimation.length;
            var elapsed = 0f;

            activationAnimation.SampleAnimation(gameObject, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                activationAnimation.SampleAnimation(gameObject, Mathf.Min(elapsed, duration));
                yield return null;
            }

            activationAnimation.SampleAnimation(gameObject, duration);
        }
        else
        {
            Debug.LogWarning($"{name}: no activation animation is assigned; activating immediately.", this);
        }

        // Some spawn paths instantiate Mimics directly instead of going through
        // HordeManager.SetupEnemy, so initialize their combat stats before enabling AI.
        _enemyStatistics.Initialize();

        RestoreEnemyColliders();

        if (_enemySelector != null)
            _enemySelector.enabled = true;

        _animator.enabled = true;

        ReattachNavMeshAgent();

        var player = Player.Instance;
        if (player != null)
            _enemyBrain.ForceTargetIndefinitely(player.transform);

        _enemyBrain.enabled = true;
        _state = MimicState.Active;
        _enemySounds?.SetIdleSoundsEnabled(true);
        _activationCoroutine = null;

        var hordeManager = HordeManager.Instance;
        if (hordeManager != null &&
            hordeManager.CurrentMoon != null &&
            hordeManager.CurrentMoon.ObjectiveType == MoonObjectiveType.FindMimics &&
            hordeManager.CurrentObjectiveProgress < hordeManager.CurrentObjectiveTarget)
        {
            hordeManager.AddObjectiveProgress(1);
        }
    }

    private void ReattachNavMeshAgent()
    {
        if (_navMeshAgent == null)
            return;

        if (!_navMeshAgent.enabled)
            _navMeshAgent.enabled = true;

        if (!_navMeshAgent.isOnNavMesh)
        {
            if (!NavMesh.SamplePosition(
                    transform.position,
                    out var navMeshHit,
                    3f,
                    _navMeshAgent.areaMask) ||
                !_navMeshAgent.Warp(navMeshHit.position))
            {
                Debug.LogWarning(
                    $"{name}: couldn't reattach the Mimic to the NavMesh after activation.",
                    this);
                return;
            }
        }

        _navMeshAgent.isStopped = false;
    }

    private void HandleEnemyDeath(EnemyStatistics enemy)
    {
        if (_state == MimicState.Dying)
            return;

        _state = MimicState.Dying;
        UnregisterForInteraction();

        if (_activationCoroutine != null)
        {
            StopCoroutine(_activationCoroutine);
            _activationCoroutine = null;
        }

        StartCoroutine(ReplaceWithChestAfterDeath());
    }

    private IEnumerator ReplaceWithChestAfterDeath()
    {
        yield return new WaitForSeconds(GetDeathAnimationDuration());

        if (chestPrefab == null)
        {
            Debug.LogError($"{name}: no chest prefab is assigned; the Mimic cannot be replaced with a chest.", this);
            yield break;
        }

        SpawnChestWithUniqueId();
        Destroy(gameObject);
    }

    private float GetDeathAnimationDuration()
    {
        var controller = _animator.runtimeAnimatorController;
        if (controller is AnimatorOverrideController overrideController)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            foreach (var clipPair in overrides)
            {
                if (clipPair.Key != null &&
                    clipPair.Key.name.IndexOf("Dead", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var deathClip = clipPair.Value != null ? clipPair.Value : clipPair.Key;
                    return deathClip.length;
                }
            }
        }

        foreach (var clip in controller != null ? controller.animationClips : Array.Empty<AnimationClip>())
        {
            if (clip != null && clip.name.IndexOf("Die", StringComparison.OrdinalIgnoreCase) >= 0)
                return clip.length;
        }

        return FallbackDeathAnimationDuration;
    }

    private void SpawnChestWithUniqueId()
    {
        var inactiveStagingObject = new GameObject("Mimic Chest Staging");
        inactiveStagingObject.SetActive(false);

        var chest = Instantiate(
            chestPrefab,
            transform.position,
            transform.rotation,
            inactiveStagingObject.transform);

        var chestInteraction = chest.GetComponentInChildren<ChestInteraction>(true);
        if (chestInteraction != null)
        {
            chestInteraction.chestId = Guid.NewGuid().ToString();
        }
        else
        {
            Debug.LogWarning($"{chest.name}: the assigned chest prefab has no ChestInteraction component.", chest);
        }

        chest.transform.SetParent(null, true);
        Destroy(inactiveStagingObject);
    }

    private void RegisterForInteraction()
    {
        if (_isRegisteredForInteraction)
            return;

        if (_interactionManager == null)
            _interactionManager = FindFirstObjectByType<InteractionManager>();

        if (_interactionManager == null)
            return;

        _interactionManager.RegisterInteractable(this);
        _isRegisteredForInteraction = true;
    }

    private void UnregisterForInteraction()
    {
        if (!_isRegisteredForInteraction)
            return;

        if (_interactionManager != null)
            _interactionManager.UnregisterInteractable(this);

        _isRegisteredForInteraction = false;
    }

    private void RestoreEnemyColliders()
    {
        if (_colliders == null || _colliderEnabledStates == null)
            return;

        for (var i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null)
                _colliders[i].enabled = _colliderEnabledStates[i];
        }
    }
}
