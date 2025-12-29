using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player _player;
    public Player Player => _player;

    protected override void Awake()
    {
        base.Awake();
        if (!_player)
        {
            _player = FindFirstObjectByType<Player>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _player.ResetPlayer();
        }
    }
}
