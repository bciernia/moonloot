using System;
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
    
    public bool IsNearBlacksmith { get; set; }
    

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

        _playerStats.ResetPlayerStats();
        
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

        var stats = new PlayerStatsData
        {
            level = _playerStats.Level,

            hp = _playerStats.HP,
            maxHP = _playerStats.MaxHP,

            mp = _playerStats.MP,
            maxMP = _playerStats.MaxMP,

            stamina = _playerStats.Stamina,
            maxStamina = _playerStats.MaxStamina,

            exp = _playerStats.Exp,
            nextLevelExp = _playerStats.NextLevelExp,

            baseDamage = _playerStats.BaseDamage,
            totalDamage = _playerStats.TotalDamage,

            physicalResistance = _playerStats.PhysicalResistance,
            magicResistance = _playerStats.MagicResistance,

            shieldResistance = _playerStats.ShieldResistance,

            // weaponID = _playerStats.currentWeapon != null
            //     ? _playerStats.currentWeapon.Id
            //     : null
        };

        ES3.Save("player_stats", stats);
    }

    public void Load()
    {
        if (ES3.KeyExists("player_position"))
        {
            transform.position = ES3.Load<Vector3>("player_position");
        }

        if (!ES3.KeyExists("player_stats"))
            return;

        var data = ES3.Load<PlayerStatsData>("player_stats");

        _playerStats.Level = data.level;

        _playerStats.HP = data.hp;
        _playerStats.MaxHP = data.maxHP;

        _playerStats.MP = data.mp;
        _playerStats.MaxMP = data.maxMP;

        _playerStats.Stamina = data.stamina;
        _playerStats.MaxStamina = data.maxStamina;

        _playerStats.Exp = data.exp;
        _playerStats.NextLevelExp = data.nextLevelExp;

        _playerStats.BaseDamage = data.baseDamage;
        _playerStats.TotalDamage = data.totalDamage;

        _playerStats.PhysicalResistance = data.physicalResistance;
        _playerStats.MagicResistance = data.magicResistance;

        _playerStats.ShieldResistance = data.shieldResistance;

        // if (!string.IsNullOrEmpty(data.weaponID))
        // {
        //     _playerStats.currentWeapon = WeaponDatabase.Get(data.weaponID);
        // }

        PlayerHealth.RefreshResistanceUI();
    }
}

[Serializable]
public class PlayerStatsData
{
    public int level;

    public float hp;
    public float maxHP;

    public float mp;
    public float maxMP;

    public float stamina;
    public float maxStamina;

    public float exp;
    public float nextLevelExp;

    public float baseDamage;
    public float totalDamage;

    public float physicalResistance;
    public float magicResistance;

    public float shieldResistance;

    public string weaponID;
}
