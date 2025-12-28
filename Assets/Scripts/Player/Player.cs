using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, ISaveable
{
    public static Player Instance { get; private set; }
    
    [Header("Configuration")] [SerializeField]
    private PlayerStatsSO _playerStats;

    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerMana PlayerMana { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }

    public string areaTransitionName;
    
    public PlayerStatsSO PlayerStats => _playerStats;
    private PlayerAnimations _playerAnimations;
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerMana = GetComponent<PlayerMana>();
        PlayerAttack = GetComponent<PlayerAttack>();
        _playerAnimations = GetComponent<PlayerAnimations>();
        _playerInput = GetComponent<PlayerInput>();

        foreach (var map in _playerInput.actions.actionMaps)
        {
            map.Enable();
        }
    }

    public void ResetPlayer()
    {
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }

    public void Save()
    {
        ES3.Save("player_position", transform.position);
    }

    public void Load()
    {
        if (!ES3.KeyExists("player_position"))
            return;
        
        transform.position = ES3.Load<Vector3>("player_position");
    }
}
