using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player _player;
    public Player Player => _player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _player.ResetPlayer();
        }
    }
}
