using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player _player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _player.ResetPlayer();
        }
    }
}
