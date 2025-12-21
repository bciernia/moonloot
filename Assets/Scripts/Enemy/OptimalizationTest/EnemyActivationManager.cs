using System.Collections.Generic;
using UnityEngine;

public class EnemyActivationManager : MonoBehaviour
{
    public static EnemyActivationManager Instance { get; private set; }

    [Tooltip("Radius around player within which enemies are active (meters)")]
    public float activationRadius = 15f;

    [Tooltip("How often (seconds) to check distances")]
    public float checkInterval = 0.2f;

    [Tooltip("If true, manager finds enemies on Start; otherwise register manually from EnemyActivatable")]
    public bool autoFindEnemies = true;

    private float _activationRadiusSqr;
    private Transform _player;
    private readonly List<EnemyActivatable> _enemies = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        _activationRadiusSqr = activationRadius * activationRadius;
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (autoFindEnemies)
        {
            var found = FindObjectsByType<EnemyActivatable>(FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
                Register(found[i]);
        }

        InvokeRepeating(nameof(CheckEnemies), 0f, checkInterval);
    }

    public void Register(EnemyActivatable activatable)
    {
        if (activatable == null) return;
        if (!_enemies.Contains(activatable))
            _enemies.Add(activatable);
    }

    public void Unregister(EnemyActivatable activatable)
    {
        if (activatable == null) return;
        _enemies.Remove(activatable);
    }

    private void CheckEnemies()
    {
        if (_player == null) return;

        Vector3 playerPos = _player.position;

        for (int i = 0; i < _enemies.Count; i++)
        {
            var e = _enemies[i];
            if (e == null) continue;

            // porównanie kwadratów odległości
            float distSqr = (e.transform.position - playerPos).sqrMagnitude;
            bool shouldBeActive = distSqr <= _activationRadiusSqr;

            if (shouldBeActive && !e.IsActive)
                e.Activate();
            else if (!shouldBeActive && e.IsActive)
                e.Deactivate();
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(CheckEnemies));
        if (Instance == this) Instance = null;
    }
}
