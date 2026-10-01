using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public PlayerExp PlayerExp { get; private set; }
    
    public bool IsNearBlacksmith { get; set; }
    

    public string areaTransitionName;
    
    
    public PlayerStatsSO PlayerStats => _playerStats;
    private PlayerAnimations _playerAnimations;
    private PlayerInput _playerInput;
    private readonly Dictionary<BonusType, Coroutine> _timedStatEffects = new();
    
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
        PlayerExp = GetComponent<PlayerExp>();
        _playerAnimations = GetComponent<PlayerAnimations>();
        _playerInput = GetComponent<PlayerInput>();

        var rebinds = PlayerPrefs.GetString("rebinds");
        
        if (!string.IsNullOrEmpty(rebinds))
        {
            _playerInput.actions.LoadBindingOverridesFromJson(rebinds);
        }
        
        _playerStats.ResetPlayerStats();
        
        foreach (var map in _playerInput.actions.actionMaps)
        {
            map.Enable();
        }
    }

    public void ResetPlayer()
    {
        foreach (var effect in _timedStatEffects.Values)
        {
            StopCoroutine(effect);
        }

        _timedStatEffects.Clear();
        _playerStats.ResetTemporaryBonuses();
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }

    public void ApplyTimedStatBonus(StatBonus bonus, float duration)
    {
        if (bonus == null || duration <= 0f)
            return;

        if (_timedStatEffects.TryGetValue(bonus.Type, out var currentEffect))
            StopCoroutine(currentEffect);

        _playerStats.SetTemporaryBonus(bonus);
        _timedStatEffects[bonus.Type] = StartCoroutine(
            RemoveTimedStatBonus(bonus.Type, duration));

        if (bonus.Type == BonusType.Damage)
            PlayerAttack.RecalculateDamage();

        if (bonus.Type == BonusType.MoveSpeed)
            PlayerStatisticsManager.Instance.SetMoveSpeed(
                _playerStats.GetMoveSpeedMultiplier());
    }

    private IEnumerator RemoveTimedStatBonus(BonusType type, float duration)
    {
        yield return new WaitForSeconds(duration);

        _playerStats.RemoveTemporaryBonus(type);
        _timedStatEffects.Remove(type);

        if (type == BonusType.Damage)
            PlayerAttack.RecalculateDamage();

        if (type == BonusType.MoveSpeed)
            PlayerStatisticsManager.Instance.SetMoveSpeed(
                _playerStats.GetMoveSpeedMultiplier());

        if (type == BonusType.MaxHp)
            PlayerHealth.ClampHealth();

        if (type == BonusType.MaxMp)
            _playerStats.MP = Mathf.Min(
                _playerStats.MP,
                _playerStats.GetMaxMp());
    }
    
    public void Save()
    {
        // ES3.Save("player_position", transform.position);

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

        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("player_stats", stats, settings);
    }

    public void Load()
    {
        // if (ES3.KeyExists("player_position"))
        // {
            // transform.position = ES3.Load<Vector3>("player_position");
        // }

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
