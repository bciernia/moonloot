using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player _player;
    public Player Player => _player;
    public static event Action<GameMode> OnGameModeChanged;

    protected override void Awake()
    {
        base.Awake();
        if (!_player)
        {
            _player = FindFirstObjectByType<Player>();
        }
    }

    // private void Update()
    // {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
            // _player.ResetPlayer();
        // }
    // }
    

    public GameMode CurrentMode { get; private set; } = GameMode.Location;

    public void SetMode(GameMode newMode)
    {
        if (CurrentMode == newMode) return;

        CurrentMode = newMode;
        OnGameModeChanged?.Invoke(newMode);
    }
}
